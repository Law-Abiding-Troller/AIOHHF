using System;
using System.Collections;
using System.Collections.Generic;
using AIOHHF.Items.Upgrades;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using AIOHHF.Mono;
using Object = UnityEngine.Object;

namespace AIOHHF.Items.Equipment;

public class AllInOneHandHeldFabricator
{
    public static Dictionary<CraftTree.Type, TechType> CustomFabricators = new();
    //public static Dictionary<CraftNode, CraftTree.Type> Fabricators = new();
    public static Dictionary<TechType, CraftNode> Nodes = new();
    public PrefabInfo PrefabInfo;
    public CustomPrefab Prefab;
    public Vector3 PostScaleValue;
    public CraftTree.Type TreeType;
    private CraftNode _nodeRoot;
    public static List<CraftNode> Trees = new();
    public static readonly List<UpgradesPrefabs>  Upgrades =  new();
    public AssetBundle Bundle;
    internal static bool Registered;
    internal static bool LateRegistered = false;
    public const string StorageName = "AIOHHFStorageChild";
    public const string StorageClassID = "AIOHHFStorageClassID";
    public IEnumerator Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Initializing All In One Hand Held Fabricator...";
        yield return null;
        if (Registered) yield break;
        Registered = true;
        Prefab.CreateFabricator(out TreeType)
            .Root.CraftTreeCreation = () =>
        {
            const string schemeId = "AIOHHFCraftTree";
            return new CraftTree(schemeId, _nodeRoot);
        };
        
        var clone = new FabricatorTemplate(PrefabInfo, TreeType)
        {
            FabricatorModel = FabricatorTemplate.Model.Fabricator,
            ModifyPrefab = prefab =>
            {
                Plugin.Aiohhf.PostScaleValue = prefab.transform.localScale = Vector3.one / 2f;
                var fab = prefab.GetComponent<Fabricator>();
                if (fab != null)
                {
                    var hhf = prefab.AddComponent<AioHandHeldFabricator>().CopyComponent(fab);
                    Object.Destroy(fab);
                    hhf.craftTree = Plugin.Aiohhf.TreeType;
                }
                prefab.AddComponent<Pickupable>();
                prefab.AddComponent<Rigidbody>();
                PrefabUtils.AddWorldForces(prefab, 5f);
                
                var child = new GameObject(StorageName);
                child.transform.SetParent(prefab.transform, false);
                var cOI = child.AddComponent<ChildObjectIdentifier>();
                cOI.ClassId = StorageClassID;
        
                var component = prefab.AddComponent<ModdedDataChipContainer>();
                var slots = new string[4];
                for (var i = 0; i < 4; i++)
                {
                    var str = Plugin.EquipmentType.ToString() + (i + 1);
                    slots[i] = str;
                }
                DataTypes.Slots.Add(new DataTypes(slots,PrefabInfo.TechType));
                DataTypes.Equipment.Add(PrefabInfo.TechType, slots);
                DataTypes.Labels.Add(PrefabInfo.TechType, Language.main.Get("PanelLabel"));
                DataTypes.ChildObjects.Add(PrefabInfo.TechType, StorageName);
                
                List<TechType> compatBats = new List<TechType>()
                {
                    TechType.Battery,
                    TechType.PrecursorIonBattery
                };
                prefab.AddComponent<HandHeldRelay>().dontConnectToRelays = true;
                PrefabUtils.AddEnergyMixin<HandHeldBatterySource>(prefab, 
                    "'I don't really get why it exists, it just decreases the chance of a collision from like 9.399613e-55% to like 8.835272e-111%, both are very small numbers' - Lee23" +
                    "(i forgot that i made my upgradeslib hand held fabricator the same storage root class id 😭 - written by lad)", 
                    TechType.Battery, compatBats);
                prefab.AddComponent<AiohhPlayerTool>();
                var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer == null) return; 
                var texture = Bundle.LoadAsset<Texture>("AIOHHF_diffuse_and_spec");
                if (texture == null) return;
                renderer.material.mainTexture = texture;
                renderer.material.SetTexture(ShaderPropertyID._SpecTex, texture);
                var actualModel = prefab.FindChild("submarine_fabricator_01");
                PrefabUtils.AddVFXFabricating(prefab, actualModel.name, -0.1f,0.2f, new Vector3(0,0.05f,0), 0.5f, new Vector3(-90,0,0));
                var fpModel = prefab.AddComponent<FPModel>();
                fpModel.viewModel = actualModel;
                var copy = Object.Instantiate(actualModel, prefab.transform);
                fpModel.propModel = copy;
                actualModel.transform.localEulerAngles = new Vector3(0,180,0);
                actualModel.transform.localPosition = new Vector3(0, 0, 0.15f);
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

        
        yield return Fragments.Initialize(task);
        task.Status = "Initializing All In One Hand Held Fabricator...";
        yield return null;
        Prefab.SetEquipment(EquipmentType.Hand);
        Prefab.Register();
    }

    public IEnumerator RegisterPrefab(WaitScreenHandler.WaitScreenTask task)
    {
        if (Registered)
        { yield break;}
        _nodeRoot = new CraftNode("Root");
            foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
            {
                //skip stuff that either throws exceptions, is my own tree, or is an unused tree
                if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                    treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 ||
                    treeType == CraftTree.Type.Rocket || treeType == TreeType
                    || treeType == CraftTree.Type.Centrifuge) continue;
                
                //techtype to set with a scope outside of each if statement
                TechType techType;
                //get the craft tree's techtype
                if (!TechTypeExtensions.FromString(treeType.ToString(), out techType, false)
                    && treeType != CraftTree.Type.MapRoom && treeType != CraftTree.Type.SeamothUpgrades) continue;
                //get the techtypes for outliers because there is no techtype of "MapRoom" or "SeamothUpgrades"
                if (techType == TechType.None)
                    techType = treeType == CraftTree.Type.SeamothUpgrades
                        ? TechType.BaseUpgradeConsole
                        : TechType.BaseMapRoom;
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
            var precursorNode = CraftTreeMethods.RegisterPrecursorFabricatorUpgrade();
            if (!precursorNode.id.Equals("NRE")) _nodeRoot.AddNode(precursorNode);
        

            yield return Initialize(task);
    }
    
    public IEnumerator LateRegister(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Adding custom fabricators to AIOHHF...";
        yield return null;
        if (LateRegistered) yield break;
        foreach (CraftNode node in CraftTreeMethods.RegisterCustomFabricatorUpgrades())
        {
            _nodeRoot.AddNode(node);
            yield return null;
        }
        var nodes = _nodeRoot.FindNodeById("Fabricator_Tools");
        var tech = Plugin.Aiohhf.PrefabInfo.TechType; 
        foreach (var upgrade in Upgrades)
        {
            if (upgrade.PrefabInfo.TechType == TechType.None) continue;
            var upgradeNode = new CraftNode("Fabricator_" + upgrade.PrefabInfo.ClassID, TreeAction.Craft,
                upgrade.PrefabInfo.TechType);
            nodes.AddNode(upgradeNode);
            yield return null;
        }
        var noder = new CraftNode("Fabricator_AIOHHF", TreeAction.Craft, tech);
        nodes.AddNode(noder);
    }
}