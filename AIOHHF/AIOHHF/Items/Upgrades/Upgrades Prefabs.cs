using System.Collections.Generic;
using AIOHHF.Items.Equipment;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Handlers;
using UnityEngine;

namespace AIOHHF.Items.Upgrades;

public class UpgradesPrefabs
{
    public CustomPrefab Prefab;
    public PrefabInfo PrefInf;
    public CraftNode Tree;//TODO: Rip Data_Box_chip model from CAB-21e70d026be83ede5b73dcbd893aac2d
    public string unknownTechType;
    public static readonly Dictionary<string, UpgradesPrefabs> AwaitingTreeCatch =  new();
    public UpgradesPrefabs(string classId, CraftNode tree, RecipeData data, TechType techType, bool unlAtStart = false)
    {
        PrefInf = PrefabInfo.WithTechType(classId, null, null, Language.main.GetCurrentLanguage())
            .WithIcon(SpriteManager.Get(techType));
        Prefab = new CustomPrefab(PrefInf);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefInf.TechType, Tree);
        var clone = new CloneTemplate(PrefInf, TechType.CyclopsShieldModule);
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
        .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
        .WithCraftingTime(3f);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.SetUnlock(unlAtStart ? Plugin.Aiohhf.PrefabInfo.TechType : techType);
        Prefab.Register();
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefInf.ClassID} registered!");
    }
    public UpgradesPrefabs(string classId, CraftNode tree, RecipeData data, Sprite sprite, bool unlAtStart = false)
    {
        PrefInf = PrefabInfo.WithTechType(classId, null,null, Language.main.GetCurrentLanguage(), unlAtStart).WithIcon(sprite);
        Prefab = new CustomPrefab(PrefInf);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefInf.TechType, Tree);
        var clone = new CloneTemplate(PrefInf, TechType.VehiclePowerUpgradeModule);
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
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefInf.ClassID} registered!");
    }

    public UpgradesPrefabs(string classId, string title, string desc, CraftNode tree, RecipeData data, TechType techType, string lang = "English", bool unlAtStart = false)
    {
        PrefInf = PrefabInfo.WithTechType(classId, title, desc, lang, unlAtStart).WithIcon(
                SpriteManager.Get(techType));
        Prefab = new CustomPrefab(PrefInf);
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefInf.TechType, Tree);
        var clone = new CloneTemplate(PrefInf, TechType.CyclopsShieldModule);
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
            .WithCraftingTime(3f);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.SetUnlock(techType);
        Prefab.Register();
        if (!Plugin.CustomFabricatorCache.TechTypeToCustomFabricator.ContainsKey(techType.AsString()))
            Plugin.CustomFabricatorCache.TechTypeToCustomFabricator.Add(techType.AsString(),new[]{classId,title,desc,unlAtStart.ToString()});
        Plugin.CustomFabricatorCache.Save(); 
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefInf.ClassID} registered!");
    }

    public UpgradesPrefabs(string techType, string[] availableInfo)
    {
        unknownTechType = techType;
        PrefInf = PrefabInfo.WithTechType(availableInfo[0],
            Language.main.Get("AwaitingTitle"),Language.main.Get("AwaitingDesc"), 
            Language.main.GetCurrentLanguage(), bool.Parse(availableInfo[3]))
            .WithIcon(SpriteManager.Get(TechType.Fabricator));
        Prefab = new CustomPrefab(PrefInf);
        Prefab.SetGameObject(new CloneTemplate(PrefInf, TechType.CyclopsShieldModule));
        Prefab.AddGadget(new CraftingGadget(Prefab, null)).WithFabricatorType(CraftTree.Type.Fabricator)
            .WithStepsToFabricatorTab("Personal", "AIOHHFTab")
            .WithCraftingTime(3f);
        Prefab.SetEquipment(Plugin.EquipmentType);
        Prefab.Register();
        AwaitingTreeCatch.Add(techType, this);
    }

    public void OnCraftTreeAcquired(string classId, string title, string desc, CraftNode tree, RecipeData data, TechType techType, string lang = "English", bool unlAtStart = false)
    {
        var name = PrefInf.ClassID;
        PrefInf.ClassID = classId;
        if (!string.IsNullOrEmpty(title))
        {
            LanguageHandler.SetLanguageLine(name, title, lang);
        }
        if (!string.IsNullOrEmpty(desc))
        {
            LanguageHandler.SetLanguageLine("Tooltip_" + name, desc, lang);
        }
        PrefInf.WithIcon(SpriteManager.Get(techType));
        Tree = tree;
        AllInOneHandHeldFabricator.Nodes.Add(PrefInf.TechType, Tree);
        CraftDataHandler.SetRecipeData(PrefInf.TechType, data);
        KnownTechHandler.SetAnalysisTechEntry(techType, new[] { PrefInf.TechType });
        if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"Prefab {PrefInf.ClassID} Successfully loaded!");
    }
}