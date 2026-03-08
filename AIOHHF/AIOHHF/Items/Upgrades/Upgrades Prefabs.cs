using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AIOHHF.Items.Equipment;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Utility;
using UnityEngine;
using UWE;

namespace AIOHHF.Items.Upgrades;

public class UpgradesPrefabs
{
    public CustomPrefab Prefab;
    public PrefabInfo PrefabInfo;
    public CraftNode Tree;//TODO: Rip Data_Box_chip model from CAB-21e70d026be83ede5b73dcbd893aac2d
    public UpgradesPrefabs(string classId, CraftNode tree, RecipeData data, TechType techType, bool unlAtStart = false)
    {
        PrefabInfo = PrefabInfo.WithTechType(classId, null, null, Language.main.GetCurrentLanguage())
            .WithIcon(SpriteManager.Get(techType));
        Prefab = new CustomPrefab(PrefabInfo);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefabInfo.TechType, Tree);
        var clone = new CloneTemplate(PrefabInfo, TechType.CyclopsShieldModule);
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
        .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
        .WithCraftingTime(3f);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.SetUnlock(unlAtStart ? Plugin.Aiohhf.PrefabInfo.TechType : techType);
        Prefab.Register();
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefabInfo.ClassID} registered!");
    }
    public UpgradesPrefabs(string classId, CraftNode tree, RecipeData data, Sprite sprite, bool unlAtStart = false)
    {
        PrefabInfo = PrefabInfo.WithTechType(classId, null,null, Language.main.GetCurrentLanguage(), unlAtStart).WithIcon(sprite);
        Prefab = new CustomPrefab(PrefabInfo);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefabInfo.TechType, Tree);
        var clone = new CloneTemplate(PrefabInfo, TechType.VehiclePowerUpgradeModule);
        clone.ModifyPrefab += obj =>
        {
            obj.gameObject.transform.localScale = Vector3.one/2;
        };
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
            .WithCraftingTime(3f);
        Prefab.SetUnlock(TechType.PrecursorIonCrystal);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.Register();
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefabInfo.ClassID} registered!");
    }
    public UpgradesPrefabs(string classId, string title, string desc, CraftNode tree, RecipeData data, TechType techType, string lang = "English", bool unlAtStart = false)
    {
        PrefabInfo = PrefabInfo.WithTechType(classId, title, desc , lang).WithIcon(SpriteManager.Get(techType));
        Prefab = new CustomPrefab(PrefabInfo);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefabInfo.TechType, Tree);
        var clone = new CloneTemplate(PrefabInfo, TechType.CyclopsShieldModule);
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
            .WithCraftingTime(3f);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.SetUnlock(unlAtStart ? Plugin.Aiohhf.PrefabInfo.TechType : techType);
        Prefab.Register();
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefabInfo.ClassID} registered!");
    }
}