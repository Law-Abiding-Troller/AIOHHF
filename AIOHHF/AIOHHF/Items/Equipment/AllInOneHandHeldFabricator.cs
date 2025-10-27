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
using AIOHHF.Mono;

namespace AIOHHF.Items.Equipment;

public class AllInOneHandHeldFabricator
{
    public static Dictionary<CraftTree.Type, TechType> CustomFabricators = new();
    public static Dictionary<CraftNode, CraftTree.Type> Fabricators = new();
    public static Dictionary<CraftTree.Type, bool> PrefabRegisters = new();
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
                
                if (!PrefabRegisters.ContainsKey(treeType)) PrefabRegisters.Add(treeType, false);
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
                if (CustomFabricators.TryGetValue(treeType, out var customPrefab))
                {
                    CraftTreeMethods.AddIconForNode(customPrefab, craftTreeTab, schemeId);
                    CraftTreeMethods.AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
                    foreach (var craftNode in craftTreeToYoink.nodes)
                    {
                        CraftTreeMethods.AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                        craftTreeTab.AddNode(craftNode);
                    }
                    nodeRoot.AddNode(craftTreeTab);
                    continue;
                }
                switch (treeType)
                {
                    case CraftTree.Type.Fabricator:
                        CraftTreeMethods.AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.CyclopsFabricator:
                        CraftTreeMethods.AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.MapRoom:
                        CraftTreeMethods.AddIconForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.SeamothUpgrades:
                        CraftTreeMethods.AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        break;
                    case CraftTree.Type.Workbench:
                        CraftTreeMethods.AddIconForNode(TechType.Workbench, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Workbench, craftTreeTab, schemeId);
                        break;
                }
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    CraftTreeMethods.AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                    craftTreeTab.AddNode(craftNode);
                }
                nodeRoot.AddNode(craftTreeTab);
            }
            return new CraftTree(schemeId, nodeRoot);
        };
        PrefabRegisters[TreeType] = true;
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
                
                if (!PrefabRegisters.ContainsKey(treeType)) PrefabRegisters.Add(treeType, false);
                TechType techType;
                if (treeType == CraftTree.Type.MapRoom) techType = TechType.BaseMapRoom;
                if (treeType == CraftTree.Type.SeamothUpgrades) techType = TechType.BaseUpgradeConsole;
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false) && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                if (EnumHandler.ModdedEnumExists<CraftTree.Type>(treeType.ToString())) CustomFabricators.Add(treeType, techType);
                Plugin.Logger.LogDebug(treeType.ToString());
                var craftTreeToYoink = CraftTree.GetTree(treeType);
                var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
                RecipeData data = new RecipeData();
                if (CustomFabricators.TryGetValue(treeType, out var customPrefab))
                {
                    CraftTreeMethods.AddIconForNode(customPrefab, craftTreeTab, schemeId);
                    CraftTreeMethods.AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
                    foreach (var craftNode in craftTreeToYoink.nodes)
                    {
                        CraftTreeMethods.AddIconForNode(craftTreeToYoink, craftNode, schemeId);
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
                        
                        if (!PrefabRegisters[treeType])
                        {Upgrades.Add(new UpgradesPrefabs($"{craftTreeTab.id}Upgrade",
                                $"{craftTreeTab.id} Tree Upgrade", 
                                $"{craftTreeTab.id} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                                $" Gives the fabricator the related craftig tree.", craftTreeTab, data));
                            PrefabRegisters[treeType] = true;
                        }
                        continue;
                    }
                    data = CraftDataHandler.GetModdedRecipeData(techType);
                    var language1 = Language.main.Get(techType);
                    if (!PrefabRegisters[treeType])
                    {Upgrades.Add(new UpgradesPrefabs($"{language1}Upgrade",
                            $"{language1} Tree Upgrade", 
                            $"{language1} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                            $" Gives the fabricator the related craftig tree.", craftTreeTab, data,techType));
                        PrefabRegisters[treeType] = true;
                    }
                    Fabricators.Add(craftTreeTab, treeType);
                    Trees.Add(craftTreeTab);
                    continue;
                }
                TechType tech = TechType.None;
                Plugin.Logger.LogDebug(treeType.ToString());
                switch (treeType)
                {
                    case CraftTree.Type.Fabricator:
                        CraftTreeMethods.AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
                        tech = TechType.Fabricator;
                        data = CraftDataHandler.GetRecipeData(TechType.Fabricator);
                        break;
                    case CraftTree.Type.CyclopsFabricator:
                        CraftTreeMethods.AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Cyclops, craftTreeTab, schemeId);
                        tech = TechType.Cyclops;
                        data = new RecipeData(new Ingredient(TechType.Titanium, 3),
                            new Ingredient(TechType.Lithium, 2),
                            new Ingredient(TechType.AdvancedWiringKit, 1),
                            new Ingredient(TechType.ComputerChip, 1));
                        break;
                    case CraftTree.Type.MapRoom:
                        CraftTreeMethods.AddIconForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
                        tech = TechType.BaseMapRoom;
                        data = CraftDataHandler.GetRecipeData(TechType.BaseMapRoom);
                        data.Ingredients.Remove(new Ingredient(TechType.Titanium, 5));
                        break;
                    case CraftTree.Type.SeamothUpgrades:
                        CraftTreeMethods.AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
                        tech = TechType.BaseUpgradeConsole;
                        data = CraftDataHandler.GetRecipeData(TechType.BaseUpgradeConsole);
                        break;
                    case CraftTree.Type.Workbench:
                        CraftTreeMethods.AddIconForNode(TechType.Workbench, craftTreeTab, schemeId);
                        CraftTreeMethods.AddLanguageForNode(TechType.Workbench, craftTreeTab, schemeId);
                        tech = TechType.Workbench;
                        data = CraftDataHandler.GetRecipeData(TechType.Workbench);
                        break;
                }
                foreach (var craftNode in craftTreeToYoink.nodes)
                {
                    CraftTreeMethods.AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                    craftTreeTab.AddNode(craftNode);
                }
                var language = Language.main.Get(tech);
                if (!PrefabRegisters[treeType])
                {
                    Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                        $"{language} Tree Upgrade",
                        $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                        $" Gives the fabricator the related craftig tree.", craftTreeTab, data, tech));
                    PrefabRegisters[treeType] = true;
                    Fabricators.Add(craftTreeTab, treeType);
                    Trees.Add(craftTreeTab);
                }
                Plugin.Logger.LogDebug(treeType.ToString());
            }
            if (!PrefabRegisters.ContainsKey(TreeType)) PrefabRegisters.Add(TreeType, false);
            if (!PrefabRegisters[TreeType]) Initialize();
        yield return null;
    }
}