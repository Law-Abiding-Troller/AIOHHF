using System;
using System.Collections.Generic;
using AIOHHF.Items.Equipment;
using AIOHHF.Items.Upgrades;
using BepInEx;
using Nautilus.Crafting;
using Nautilus.Handlers;
using UnityEngine;

namespace AIOHHF.Mono;

public static class CraftTreeMethods
{
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
            foreach (var unused in node)
            {
                string origLanguage;
                origLanguage = Language.main.Get(origID == null ? $"{origTreeScheme.id}Menu_{node.id}" : $"{origTreeScheme.id}Menu_{origID}");
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
                if (Plugin.ConfigFile.DebugMode) Plugin.Logger.LogDebug($"{origTitle} is either null or whitespace for {techType}!");
            }
        }
    }

    public static CraftNode RegisterFabricatorUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.Fabricator);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"FabricatorUpgrade",
                craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.Fabricator), TechType.Fabricator));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Fabricator);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterPrecursorFabricatorUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        CraftTree.Type treeType = CraftTree.Type.None;
        foreach (CraftTree.Type tree in Enum.GetValues(typeof(CraftTree.Type)))
        {
            if (!AllInOneHandHeldFabricator.CustomFabricators.TryGetValue(tree, out var cusFabricator)) continue;
            if (cusFabricator.ToString().Equals("ProtoPrecursorFabricator")) treeType = tree;
        }

        if (!TechTypeExtensions.FromString(treeType.ToString(), out var tech, false))
            return new CraftNode("NRE");
        if (!AllInOneHandHeldFabricator.CustomFabricators.TryGetValue(treeType, out var customFabricator))
            return new CraftNode("NRE");
        var craftTreeToYoink = CraftTree.GetTree(treeType);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        var sprite = Plugin.Aiohhf.Bundle.LoadAsset<Sprite>("PrecursorFab");
        if (sprite != null) SpriteHandler.RegisterSprite(SpriteManager.Group.Category, $"{schemeId}_{craftTreeTab.id}", sprite);
        AddLanguageForNode(tech, craftTreeTab, schemeId);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!TechTypeExtensions.FromString("AlienBuildingBlock", out var buildingBlock, false)) return new CraftNode("NRE");
        if (!TechTypeExtensions.FromString("IonPrism", out var ionPrism, false)) return new CraftNode("NRE");
        if (!TechTypeExtensions.FromString("Proto_PrecursorIngot", out var precursorIngot,false)) return new CraftNode("NRE");
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"PrototypeUpgrade", 
                craftTreeTab, 
                new RecipeData(
                    new Ingredient(buildingBlock, 1),
                    new Ingredient(ionPrism, 1),
                    new Ingredient(precursorIngot, 1),
                    new Ingredient(TechType.PrecursorIonCrystalMatrix, 1)), 
                sprite));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, treeType);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }

    public static List<CraftNode> RegisterCustomFabricatorUpgrades()
    {
        const string schemeId = "AIOHHFCraftTree";
        List<CraftNode> craftNodes = new List<CraftNode>();
        foreach (CraftTree.Type treeType in AllInOneHandHeldFabricator.CustomFabricators.Keys)
        {
            if (AllInOneHandHeldFabricator.CustomFabricators[treeType].ToString().Equals("ProtoPrecursorFabricator")) continue;
            TechType customPrefab = AllInOneHandHeldFabricator.CustomFabricators[treeType];
            var craftTreeToYoink = CraftTree.GetTree(treeType);
            var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
            AddIconForNode(customPrefab, craftTreeTab, schemeId);
            AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
            foreach (var craftNode in craftTreeToYoink.nodes)
            {
                AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                craftTreeTab.AddNode(craftNode);
            }
            var techType = Language.main.Get(customPrefab);
            if (UpgradesPrefabs.AwaitingTreeCatch.TryGetValue(customPrefab.AsString(), out var caughtPrefab))
            {
                caughtPrefab.OnCraftTreeAcquired(techType.Replace(" ", "") + Language.main.Get("CustomFabricatorClassID"), 
                    techType + Language.main.Get("CustomFabricator"), 
                    techType + Language.main.Get("Tooltip_CustomFabricator"), craftTreeTab, 
                    CraftDataHandler.GetModdedRecipeData(customPrefab), customPrefab, Language.main.GetCurrentLanguage());
            }
            else 
                AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs(
                techType.Replace(" ", "") + Language.main.Get("CustomFabricatorClassID"), 
                techType + Language.main.Get("CustomFabricator"), 
                techType + Language.main.Get("Tooltip_CustomFabricator"), craftTreeTab, 
                    CraftDataHandler.GetModdedRecipeData(customPrefab), customPrefab, Language.main.GetCurrentLanguage()));
            //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Fabricator);
            AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
            craftNodes.Add(craftTreeTab);
        }
        return craftNodes;
    }
    
    public static CraftNode RegisterWorkbenchUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.Workbench);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.Workbench, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.Workbench, craftTreeTab, schemeId);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"WorkbenchDataChip", 
                craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.Workbench), TechType.Workbench));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Workbench);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterCyclopsFabricatorUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.CyclopsFabricator);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId); 
        LanguageHandler.SetLanguageLine($"{schemeId}Menu_{craftTreeTab.id}",Language.main.Get("CyclopsNode"));
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"CyclopsDataChip",
                craftTreeTab, 
                new RecipeData(new Ingredient(TechType.Titanium, 3),
                    new Ingredient(TechType.Lithium, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.ComputerChip, 1)), TechType.Cyclops));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.CyclopsFabricator);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterVehicleUpgradeConsoleUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.SeamothUpgrades);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"VUCDataChip",
                craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.BaseUpgradeConsole), TechType.BaseUpgradeConsole));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.SeamothUpgrades);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterScannerRoomUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.MapRoom);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.BaseMapRoom, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.BaseMapRoom, craftTreeTab, schemeId);
        var language = Language.main.Get(TechType.BaseMapRoom);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"ScannerRoomDataChip",
                craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.BaseMapRoom), TechType.BaseMapRoom));
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.MapRoom);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
}