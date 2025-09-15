using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AIOHHF.Items.Equipment;

public class AIOHHF
{
    public static PrefabInfo AIOHHFPrefabInfo;
    public static CustomPrefab AIOHHFPrefab;
    public static FabricatorGadget AIOHHFFabricator;
    public static Vector3 PostScaleValue;
    public static CraftTree.Type AIOHHFTreeType;

    public static void RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Creating AIOHHF PrefabInfo and TechType";
        AIOHHFPrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
            "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held!" +
            "\nEnergy consumption is the same as a normal Fabricator")
            .WithIcon(SpriteManager.Get(TechType.Fabricator)).WithSizeInInventory(new Vector2int(2,2));
        task.Status = "Initializing AIOHHF Prefab";
        AIOHHFPrefab = new CustomPrefab(AIOHHFPrefabInfo);
        task.Status = "Creating AIOHHF Tree";
        AIOHHFFabricator = AIOHHFPrefab.CreateFabricator(out AIOHHFTreeType);
        int secondaryiterator = 0;
        foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
        {
            if (treeType == CraftTree.Type.Constructor) continue;
            task.Status = $"Creating AIOHHF Tree\nTree: {CraftTree.GetTree(treeType).id}\nIteration: {secondaryiterator}";
            AIOHHFFabricator.AddTabNode(CraftTree.GetTree(treeType).id + "AIOHHFTab",
                CraftTree.GetTree(treeType).id,
                SpriteManager.Get(TechType.Fabricator));
            CraftTree.GetTree(AIOHHFTreeType)
                .nodes[secondaryiterator]
                .AddNode(CraftTree.GetTree(treeType).nodes);
            secondaryiterator++;
        }

        task.Status = "Creating Object";
        var clone = new FabricatorTemplate(AIOHHFPrefabInfo, AIOHHFTreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            { 
                task.Status = "Creating Object\nModifying Fabricator Prefab\nScaling down by half...";
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                PostScaleValue = model.transform.localScale;
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding Pickupable Component...";
                prefab.AddComponent<Pickupable>();
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldFabricator Component...";
                prefab.AddComponent<HandHeldFabricator>();
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldRelay Component...";
                List<TechType> compatbats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldBatterySource Component...";
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23", 
                    TechType.Battery, compatbats);

            }
        };
        task.Status = "Setting Object";
        AIOHHFPrefab.SetGameObject(clone);
        task.Status = "Setting Recipe";
        AIOHHFPrefab.SetRecipe(new RecipeData()
        {
            craftAmount = 1,
            Ingredients = new List<Ingredient>()
            {
                new Ingredient(TechType.Titanium, 3),
                new Ingredient(TechType.ComputerChip, 2),
                new Ingredient(TechType.WiringKit, 1),
                new Ingredient(TechType.Diamond, 1),
                new Ingredient(TechType.AluminumOxide, 1),
                new Ingredient(TechType.Magnetite, 1)
            }
        })
        .WithFabricatorType(CraftTree.Type.Fabricator)
        .WithStepsToFabricatorTab("Personal","Tools")
        .WithCraftingTime(5f);
        task.Status = "Setting Equipment Type";
        AIOHHFPrefab.SetEquipment(EquipmentType.Hand);
        task.Status = "Setting Unlock";
        AIOHHFPrefab.SetUnlock(TechType.Peeper);
        task.Status = "Done!";
    }
}
public class HandHeldFabricator : PlayerTool
{
    public Fabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public override void Awake()
    {
        fab = gameObject.GetComponent<Fabricator>();
        relay = gameObject.GetComponent<PowerRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        battery.connectedRelay = relay;
        relay.AddInboundPower(battery);
    }
    public override bool OnRightHandDown()
    {
        Plugin.Logger.LogDebug($"OnRightHandDown: {relay.inboundPowerSources.Count},{relay.GetPower()}, {battery.connectedRelay}, {battery.enabled}, {battery.charge}");
        fab.opened = true;
        uGUI.main.craftingMenu.Open(AIOHHF.AIOHHFFabricator.CraftTreeType, fab);
        return true;
    }

    public void Update()
    {
        gameObject.transform.localScale = AIOHHF.PostScaleValue;
    }

    public override void OnDraw(Player p)
    {
        base.OnDraw(p);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, fab.state);
    }
}

public class HandHeldRelay : PowerRelay
{
    public override void Start()
    {
        InvokeRepeating("UpdatePowerState", Random.value, 0.5f);
        lastCanConnect = CanMakeConnection();
        StartCoroutine(UpdateConnectionAsync());
        UpdatePowerState();
        if (WaitScreen.IsWaiting)
        {
            lastPowered = isPowered = true;
            powerStatus = PowerSystem.Status.Normal;
        }
    }
}

public class HandHeldBatterySource : BatterySource
{
    public override void Start()
    {
        RestoreBattery();
        
    }
}