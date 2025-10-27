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
        var nodeRoot = new CraftNode("Root");
            const string schemeId = "AIOHHFCraftTree";
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 ||
                    treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;

                if (!PrefabRegisters.ContainsKey(treeType)) PrefabRegisters.Add(treeType, false);
                TechType techType;
                if (treeType == CraftTree.Type.MapRoom) techType = TechType.BaseMapRoom;
                if (treeType == CraftTree.Type.SeamothUpgrades) techType = TechType.BaseUpgradeConsole;
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false)
                    && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                if (EnumHandler.ModdedEnumExists<CraftTree.Type>(treeType.ToString()))
                    CustomFabricators.Add(treeType, techType);
            }

            nodeRoot.AddNode(CraftTreeMethods.RegisterFabricatorUpgrade());
            nodeRoot.AddNode(CraftTreeMethods.RegisterWorkbenchUpgrade());
            nodeRoot.AddNode(CraftTreeMethods.RegisterCyclopsFabricatorUpgrade());
            nodeRoot.AddNode(CraftTreeMethods.RegisterPrecursorFabricatorUpgrade());
            nodeRoot.AddNode(CraftTreeMethods.RegisterScannerRoomUpgrade());
            nodeRoot.AddNode(CraftTreeMethods.RegisterVehicleUpgradeConsoleUpgrade());
            foreach (CraftNode node in CraftTreeMethods.RegisterCustomFabricatorUpgrades())
            {
                node.AddNode(node);
            }
            if (!PrefabRegisters.ContainsKey(TreeType)) PrefabRegisters.Add(TreeType, false);
            if (!PrefabRegisters[TreeType]) Initialize();
        yield return null;
    }
}