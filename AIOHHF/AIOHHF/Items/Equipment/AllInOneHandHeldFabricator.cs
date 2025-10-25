using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net;
using AIOHHF.Items.Upgrades;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UWE;
using Random = UnityEngine.Random;

namespace AIOHHF.Items.Equipment;

public class AllInOneHandHeldFabricator
{
    private static Dictionary<CraftTree.Type, TechType> _customFabricators = new();
    private static Dictionary<CraftNode, CraftTree.Type> _fabricators = new();
    private static Dictionary<CraftTree.Type, bool> _prefabRegisters = new();
    public static PrefabInfo PrefabInfo;
    public static CustomPrefab Prefab;
    public static FabricatorGadget Fabricator;
    public static Vector3 PostScaleValue;
    public static CraftTree.Type TreeType;
    public static List<CraftNode> Trees = new List<CraftNode>();
    public static List<UpgradesPrefabs>  Upgrades =  new List<UpgradesPrefabs>();
    public static List<ICustomPrefab> RecipePrefabs = new();
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
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;
                
                if (!_prefabRegisters.ContainsKey(treeType)) _prefabRegisters.Add(treeType, false);
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
                if (_customFabricators.TryGetValue(treeType, out var customPrefab))
                {
                    AddIconForNode(customPrefab, craftTreeTab, schemeId);
                    AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
                    foreach (var craftNode in craftTreeToYoink.nodes)
                    {
                        AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                        craftTreeTab.AddNode(craftNode);
                    }
                    nodeRoot.AddNode(craftTreeTab);
                    continue;
                }
                switch (treeType)
                {
                    case CraftTree.Type.Fabricator:
                        AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.CyclopsFabricator:
                        AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.MapRoom:
                        AddIconForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.SeamothUpgrades:
                        AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.Workbench:
                        AddIconForNode(TechType.Workbench, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Workbench, craftTreeTab, schemeId);
                        break;
                }
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                    craftTreeTab.AddNode(craftNode);
                }
                nodeRoot.AddNode(craftTreeTab);
            }
            return new CraftTree(schemeId, nodeRoot);
        };
        _prefabRegisters[TreeType] = true;
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
                prefab.AddComponent<Rigidbody>();
                PrefabUtils.AddWorldForces(prefab, 5);
                PrefabUtils.AddStorageContainer(prefab, "AIOHHFStorageContainer", "ALL IN ONE HAND HELD FABRICATOR", 2 ,2);
                List<TechType> compatbats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23" +
                    "(i forgot that i made my upgradeslib hand held fabricator the same storage root class id :sob:)", 
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

    public static IEnumerator RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
            const string schemeId = "AIOHHFCraftTree";
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;
                
                if (!_prefabRegisters.ContainsKey(treeType)) _prefabRegisters.Add(treeType, false);
                if (!TechTypeExtensions.FromString(treeType.ToString(), out TechType techType, false)) continue;
                switch (treeType)
                {
                    case CraftTree.Type.SeamothUpgrades:
                        break;
                    case CraftTree.Type.Fabricator:
                        break;
                    case CraftTree.Type.CyclopsFabricator:
                        break;
                    case CraftTree.Type.MapRoom:
                        break;
                    case CraftTree.Type.Workbench:
                        break;
                    default:
                        _customFabricators.Add(treeType, techType);
                        break;
                }
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
                RecipeData data = new RecipeData();
                if (_customFabricators.TryGetValue(treeType, out var customPrefab))
                {
                    AddIconForNode(customPrefab, craftTreeTab, schemeId);
                    AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
                    foreach (var craftNode in craftTreeToYoink.nodes)
                    {
                        AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                        craftTreeTab.AddNode(craftNode);
                    }
                    if (techType.ToString().Equals("ProtoPrecursorFabricator"))
                    {
                        Plugin.Logger.LogDebug("Prototype fabricator Found!");
                        if (!TechTypeExtensions.FromString("AlienBuildingBlock", out var buildingBlock, false)) continue;
                        if (!TechTypeExtensions.FromString("IonPrism", out var ionPrism, false)) continue;
                        if (!TechTypeExtensions.FromString("Proto_PrecursorIngot", out var precursorIngot,false)) continue;
                        Plugin.Logger.LogDebug("Prototype recipe found!");
                        data = new RecipeData(
                            new Ingredient(buildingBlock, 1),
                            new Ingredient(ionPrism, 1),
                            new Ingredient(precursorIngot, 1),
                            new Ingredient(TechType.PrecursorIonCrystalMatrix, 1));
                        
                        if (!_prefabRegisters[treeType])
                        {Upgrades.Add(new UpgradesPrefabs($"{craftTreeTab.id}Upgrade",
                                $"{craftTreeTab.id} Tree Upgrade", 
                                $"{craftTreeTab.id} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                                $" Gives the fabricator the related craftig tree.", craftTreeTab, data));
                            _prefabRegisters[treeType] = true;
                        }
                        continue;
                    }
                    data = CraftDataHandler.GetModdedRecipeData(techType);
                    var language1 = Language.main.Get(techType);
                    if (!_prefabRegisters[treeType])
                    {Upgrades.Add(new UpgradesPrefabs($"{language1}Upgrade",
                            $"{language1} Tree Upgrade", 
                            $"{language1} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                            $" Gives the fabricator the related craftig tree.", craftTreeTab, data,techType));
                        _prefabRegisters[treeType] = true;
                    }
                    _fabricators.Add(craftTreeTab, treeType);
                    Trees.Add(craftTreeTab);
                    continue;
                }
                TechType tech = TechType.None;
                Plugin.Logger.LogDebug(treeType.ToString());
                switch (treeType)
                {
                    case CraftTree.Type.Fabricator:
                        AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        tech = TechType.Fabricator;
                        data = CraftDataHandler.GetRecipeData(TechType.Fabricator);
                        break;
                    case CraftTree.Type.CyclopsFabricator:
                        AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        tech = TechType.Cyclops;
                        data = new RecipeData(new Ingredient(TechType.Titanium, 3),
                            new Ingredient(TechType.Lithium, 2),
                            new Ingredient(TechType.AdvancedWiringKit, 1),
                            new Ingredient(TechType.ComputerChip, 1));
                        break;
                    case CraftTree.Type.MapRoom:
                        AddIconForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        tech = TechType.BaseMapRoom;
                        data = CraftDataHandler.GetRecipeData(TechType.BaseMapRoom);
                        data.Ingredients.Remove(new Ingredient(TechType.Titanium, 5));
                        break;
                    case CraftTree.Type.SeamothUpgrades:
                        AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        tech = TechType.BaseUpgradeConsole;
                        data = CraftDataHandler.GetRecipeData(TechType.BaseUpgradeConsole);
                        break;
                    case CraftTree.Type.Workbench:
                        AddIconForNode(TechType.Workbench, craftTreeTab, schemeId);
                        AddLanguageForNode(TechType.Workbench, craftTreeTab, schemeId);
                        tech = TechType.Workbench;
                        data = CraftDataHandler.GetRecipeData(TechType.Workbench);
                        break;
                }
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                    craftTreeTab.AddNode(craftNode);
                }
                var language = Language.main.Get(tech);
                if (!_prefabRegisters[treeType])
                {
                    Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                        $"{language} Tree Upgrade",
                        $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                        $" Gives the fabricator the related craftig tree.", craftTreeTab, data, tech));
                    _prefabRegisters[treeType] = true;
                    _fabricators.Add(craftTreeTab, treeType);
                    Trees.Add(craftTreeTab);
                }
            }
            if (!_prefabRegisters.ContainsKey(TreeType)) _prefabRegisters.Add(TreeType, false);
            if (!_prefabRegisters[TreeType]) Initialize();
        yield return null;
    }

    public static void AddIconForNode(CraftTree origTreeScheme,CraftNode node, string newTreeScheme, bool addLanguage = true)
    {
        if (node.action == TreeAction.Expand)
        {
            var originalID = node.id;
            node.id = $"{origTreeScheme.id}_{originalID}";
            var icon = SpriteManager.Get(SpriteManager.Group.Category, $"{origTreeScheme.id}_{originalID}");
            SpriteHandler.RegisterSprite(SpriteManager.Group.Category, $"{newTreeScheme}_{node.id}", icon);
            if (addLanguage) AddLanguageForNode(origTreeScheme, node, newTreeScheme, originalID);
            foreach (var nodes in node)
            {
                AddIconForNode(origTreeScheme, nodes, newTreeScheme);
            }
        }
    }
    public static void AddIconForNode(TechType treeType, CraftNode node, string schemeId)
    {
        if (node.action == TreeAction.Expand)
        {
            SpriteHandler.RegisterSprite(SpriteManager.Group.Category, $"{schemeId}_{node.id}", SpriteManager.Get(treeType));
        }
    }

    public static void AddLanguageForNode(CraftTree origTreeScheme,CraftNode node, string newTreeScheme, string origID = null)
    {
        if (node.action == TreeAction.Expand)
        {
            foreach (var nodes in node)
            {
                string origLanguage;
                if (origID == null) {origLanguage = Language.main.Get($"{origTreeScheme.id}Menu_{node.id}");}
                else {origLanguage = Language.main.Get($"{origTreeScheme.id}Menu_{origID}");}
                LanguageHandler.SetLanguageLine($"{newTreeScheme}Menu_{node.id}",origLanguage);
            }
        }
    }
    public static void AddLanguageForNode(TechType techType, CraftNode node, string newTreeScheme)
    {
        if (node.action == TreeAction.Expand)
        {
            var origTitle = Language._main.Get(techType);
            if (!origTitle.IsNullOrWhiteSpace())
                LanguageHandler.SetLanguageLine($"{newTreeScheme}Menu_{node.id}", origTitle);
            else
            {
                Plugin.Logger.LogDebug($"{origTitle} is either null or whitespace for {techType}!");
            }
        }
    }
}

public static class CustomExtentions
{
    public static bool TryGetForClassID(this List<ICustomPrefab> prefabs, string classId, out ICustomPrefab prefab)
    {
        prefab = null;
        foreach (var possiblePrefab in prefabs)
        {
            if (possiblePrefab.Info.ClassID.Equals(classId)) prefab = possiblePrefab;
            return true;
        }
        return false;
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