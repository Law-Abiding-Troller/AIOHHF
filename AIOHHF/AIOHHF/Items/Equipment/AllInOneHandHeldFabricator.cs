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

    public static void Initialize()
    {
        PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                        "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held(tm)!" +
                        "\nEnergy consumption is the same as a normal Fabricator")
                    .WithIcon(SpriteManager.Get(TechType.Fabricator)).WithSizeInInventory(new Vector2int(2,2));
        Prefab = new CustomPrefab(PrefabInfo);
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            var nodeRoot = new CraftNode("Root");
            const string schemeId = "AIOHHFCraftTree";
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == TreeType) continue;
                
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                var craftTreeTab = new CraftNode(treeType.ToString(), TreeAction.Expand);
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                    craftTreeTab.AddNode(craftNode);
                }
                nodeRoot.AddNode(craftTreeTab);
            }

            return new CraftTree(schemeId, nodeRoot);
        };
        Fabricator = Prefab.GetGadget<FabricatorGadget>();
        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            { 
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                PostScaleValue = model.transform.localScale;
                prefab.AddComponent<Pickupable>();
                prefab.AddComponent<HandHeldFabricator>();
                PrefabUtils.AddStorageContainer(prefab, "AIOHHFStorageContainer", "ALL IN ONE HAND HELD FABRICATOR", 2 ,2);
                List<TechType> compatbats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23", 
                    TechType.Battery, compatbats);

            }
        };
        Prefab.SetGameObject(clone);
        var ingredients = new List<Ingredient>()
        {
            new Ingredient(TechType.Titanium, 3),
            new Ingredient(TechType.CopperWire, 2)
        };
        Prefab.SetRecipe(new RecipeData()
            {
                craftAmount = 1,
                Ingredients = ingredients
            })
            .WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal","Tools")
            .WithCraftingTime(5f);
        Prefab.SetUnlock(TechType.Peeper);
        Prefab.SetEquipment(EquipmentType.Hand);
        Prefab.Register();
    }
    public static void RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        
        /*Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            var nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == TreeType) continue;
                
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    var currentTab = craftNode;
                    if (craftNode.action == TreeAction.Expand)
                    {
                        AddNodesUnderTabs(craftNode, currentTab);
                    }
                }
            }

            return new CraftTree("AIOHHFCraftTree", nodeRoot);
        };
        Fabricator = Prefab.GetGadget<FabricatorGadget>();
        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            { 
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                PostScaleValue = model.transform.localScale;
                prefab.AddComponent<Pickupable>();
                prefab.AddComponent<HandHeldFabricator>();
                List<TechType> compatbats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23", 
                    TechType.Battery, compatbats);

            }
        };
        Prefab.SetGameObject(clone);
        Prefab.Register();*/
    }

    public static void AddIconForNode(CraftTree origTreeScheme,CraftNode node, string newTreeScheme)
    {
        var origIcon = SpriteManager.Get(SpriteManager.Group.Category, $"{origTreeScheme.id}_{node.id}");
        SpriteHandler.RegisterSprite(SpriteManager.Group.Category, $"{newTreeScheme}_{node.id}", origIcon);
        if (node.action == TreeAction.Expand)
        {
            foreach (var nodes in node)
            {
                AddIconForNode(origTreeScheme, nodes, newTreeScheme);
            }
        }
    }
}

public class HandHeldFabricator : PlayerTool
{
    public Fabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
    public override void Awake()
    {
        fab = gameObject.GetComponent<Fabricator>();
        relay = gameObject.GetComponent<PowerRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<StorageContainer>();
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

    public override bool OnAltDown()
    {
        if (!storageContainer.open && storageContainer != null && storageContainer.container != null)
        {
            var allowedtech = new[]
            {
                TechType.PowerCell
            };
            storageContainer.container._label =  "ALL IN ONE HAND HELD FABRICATOR";
            storageContainer.container.SetAllowedTechTypes(allowedtech);
            storageContainer.Open();
        }

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