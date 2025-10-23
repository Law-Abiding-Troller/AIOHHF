using System.Collections.Generic;
using AIOHHF.Items.Equipment;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using UnityEngine;

namespace AIOHHF.Items.Upgrades;

public class UpgradesPrefabs
{
    public CustomPrefab Prefab;
    public PrefabInfo PrefabInfo;
    public CraftNode Tree;
    public UpgradesPrefabs(string classId, string name, string desc, CraftNode tree, RecipeData data, string lang = "English", bool unlAtStart = false)
    {
        PrefabInfo = Nautilus.Assets.PrefabInfo.WithTechType(classId, name, desc, lang, unlAtStart);
        Prefab = new CustomPrefab(PrefabInfo);
        Tree = tree;
        var clone = new CloneTemplate(PrefabInfo, TechType.VehiclePowerUpgradeModule);
        clone.ModifyPrefab += obj =>
        {
            obj.gameObject.transform.localScale = Vector3.one/2;
        };
        Prefab.SetGameObject(clone);
        Prefab.SetRecipe(data).WithFabricatorType(CraftTree.Type.Fabricator)
        .WithStepsToFabricatorTab("Personal", "Tools")
        .WithCraftingTime(3f);
        Prefab.SetUnlock(AllInOneHandHeldFabricator.PrefabInfo.TechType);
        Prefab.Register();
    }
}