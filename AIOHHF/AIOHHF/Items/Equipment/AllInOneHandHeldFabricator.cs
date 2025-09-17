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

public class AllInOneHandHeldFabricator
{
    public static PrefabInfo PrefabInfo;
    public static CustomPrefab Prefab;
    public static FabricatorGadget Fabricator;
    public static Vector3 PostScaleValue;
    public static CraftTree.Type TreeType;

    public static void RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Creating AIOHHF PrefabInfo and TechType";
        Plugin.Logger.LogDebug(task.Status);
        PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held!" +
                "\nEnergy consumption is the same as a normal Fabricator")
            .WithIcon(SpriteManager.Get(TechType.Fabricator)).WithSizeInInventory(new Vector2int(2,2));
        task.Status = "Initializing AIOHHF Prefab";
        Plugin.Logger.LogDebug(task.Status);
        Prefab = new CustomPrefab(PrefabInfo);
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            var nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == Items.Equipment.AllInOneHandHeldFabricator.TreeType) continue;
                var currentTreeTab = nodeRoot.AddNode(new CraftNode(nameof(treeType)));
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    var currentTab = craftNode;
                    currentTab.AddNode(currentTab);
                    if (craftTreeToYoink.nodes.action == TreeAction.Expand)
                    {
                        AddNodesUnderTabs(craftNode, currentTab);
                    }
                }
            }

            return new CraftTree("AIOHHFCraftTree", nodeRoot);
        };
        Fabricator = Prefab.GetGadget<FabricatorGadget>();
        task.Status = "Creating Object";
        Plugin.Logger.LogDebug(task.Status);
        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            { 
                task.Status = "Creating Object\nModifying Fabricator Prefab\nScaling down by half...";
                Plugin.Logger.LogDebug(task.Status);
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                PostScaleValue = model.transform.localScale;
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding Pickupable Component...";
                Plugin.Logger.LogDebug(task.Status);
                prefab.AddComponent<Pickupable>();
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldFabricator Component...";
                Plugin.Logger.LogDebug(task.Status);
                prefab.AddComponent<HandHeldFabricator>();
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldRelay Component...";
                Plugin.Logger.LogDebug(task.Status);
                List<TechType> compatbats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                task.Status = "Creating Object\nModifying Fabricator Prefab\nAdding HandHeldBatterySource Component...";
                Plugin.Logger.LogDebug(task.Status);
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23", 
                    TechType.Battery, compatbats);

            }
        };
        task.Status = "Setting Object";
        Plugin.Logger.LogDebug(task.Status);
        Prefab.SetGameObject(clone);
        task.Status = "Setting Recipe";
        Plugin.Logger.LogDebug(task.Status);
        Prefab.SetRecipe(new RecipeData()
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
        Plugin.Logger.LogDebug(task.Status);
        Prefab.SetEquipment(EquipmentType.Hand);
        task.Status = "Setting Unlock";
        Plugin.Logger.LogDebug(task.Status);
        Prefab.SetUnlock(TechType.Peeper);
        task.Status = "Registering AIOHHF Prefab";
        Plugin.Logger.LogDebug(task.Status);
        Prefab.Register();
        task.Status = "Done!";
        Plugin.Logger.LogDebug(task.Status);
    }

    public static void AddNodesUnderTabs(CraftNode tab,CraftNode root)
    {
        foreach (CraftNode pernode in tab)
        {
            if (pernode.action == TreeAction.Expand)
            {
                AddNodesUnderTabs(pernode, root);
            }
            root.AddNode(pernode);
        }
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
        uGUI.main.craftingMenu.Open(AllInOneHandHeldFabricator.Fabricator.CraftTreeType, fab);
        return true;
    }

    public void Update()
    {
        gameObject.transform.localScale = AllInOneHandHeldFabricator.PostScaleValue;
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