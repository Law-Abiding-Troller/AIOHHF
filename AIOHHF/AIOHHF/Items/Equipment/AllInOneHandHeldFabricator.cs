using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net;
using System.Reflection;
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
using Object = UnityEngine.Object;

namespace AIOHHF.Items.Equipment;

public class AllInOneHandHeldFabricator
{
    public static Dictionary<CraftTree.Type, TechType> CustomFabricators = new();
    //public static Dictionary<CraftNode, CraftTree.Type> Fabricators = new();
    public static Dictionary<CraftTree.Type, bool> PrefabRegisters = new();
    public static Dictionary<TechType, CraftNode> Nodes = new();
    public PrefabInfo PrefabInfo;
    public CustomPrefab Prefab;
    //public static FabricatorGadget Fabricator;
    public Vector3 PostScaleValue;
    public CraftTree.Type TreeType;
    private CraftNode _nodeRoot;
    //public static List<CraftNode> Trees = new();
    public static List<UpgradesPrefabs>  Upgrades =  new();
    public void Initialize()
    {
        PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                        "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held(tm)!" +
                        "\nEnergy consumption is the same as a normal Fabricator")
                    .WithIcon(SpriteManager.Get(TechType.Fabricator)).WithSizeInInventory(new Vector2int(2,2));
        Prefab = new CustomPrefab(PrefabInfo);
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            const string schemeId = "AIOHHFCraftTree";
            return new CraftTree(schemeId, _nodeRoot);
        };
        PrefabRegisters[TreeType] = true;
        //Fabricator = Prefab.GetGadget<FabricatorGadget>();
        
        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            { 
                var fab = prefab.GetComponent<Fabricator>();
                if (fab != null)
                {
                    var hhf = prefab.AddAndCopyComponent<HandHeldFabricator, Fabricator>();
                    Object.Destroy(fab);
                }
                GameObject model = prefab.gameObject; 
                model.transform.localScale = Vector3.one / 2f;
                PostScaleValue = model.transform.localScale;
                prefab.AddComponent<Pickupable>();
                prefab.AddComponent<HandHeldPlayerTool>();
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

    public void RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    { 
        _nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 ||
                    treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;

                //if its not defined, add it
                if (!PrefabRegisters.ContainsKey(treeType)) PrefabRegisters.Add(treeType, false);
                //techtype to set with a scope outside of each if statement
                TechType techType;
                //get the craft tree's techtype
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false)
                    && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                //get the techtypes for outliers because there is no techtype of "MapRoom" or "SeamothUpgrades"
                if (treeType == CraftTree.Type.MapRoom) techType = TechType.BaseMapRoom;
                if (treeType == CraftTree.Type.SeamothUpgrades) techType = TechType.BaseUpgradeConsole;
                //is it a custom craft tree?
                if (EnumHandler.ModdedEnumExists<CraftTree.Type>(treeType.ToString()))
                    //add it if so
                    CustomFabricators.Add(treeType, techType);
                //do nothing with the vanilla ones since they are mapped manually
            }

            _nodeRoot.AddNode(CraftTreeMethods.RegisterFabricatorUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterWorkbenchUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterCyclopsFabricatorUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterScannerRoomUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterVehicleUpgradeConsoleUpgrade());
            _nodeRoot.AddNode(CraftTreeMethods.RegisterPrecursorFabricatorUpgrade());
            foreach (CraftNode node in CraftTreeMethods.RegisterCustomFabricatorUpgrades())
            {
                _nodeRoot.AddNode(node);
            }
            
            if (!PrefabRegisters.ContainsKey(TreeType)) PrefabRegisters.Add(TreeType, false);
            if (!PrefabRegisters[TreeType]) Initialize();
    }
}