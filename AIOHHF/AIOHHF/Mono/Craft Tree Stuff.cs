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

    public static CraftNode RegisterFabricatorUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.Fabricator);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.Fabricator, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.Fabricator, craftTreeTab, schemeId);
        var language = Language.main.Get(TechType.Fabricator);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Fabricator])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.Fabricator), TechType.Fabricator));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Fabricator] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Fabricator);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
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
        if (!AllInOneHandHeldFabricator.CustomFabricators.TryGetValue(treeType, out var customFabricator))
            return new CraftNode("NRE");
        var craftTreeToYoink = CraftTree.GetTree(treeType);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.None, craftTreeTab, schemeId); 
        AddLanguageForNode(0, craftTreeTab, schemeId);
        var language = Language.main.Get(customFabricator);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!TechTypeExtensions.FromString("AlienBuildingBlock", out var buildingBlock, false)) return new CraftNode("NRE");
        if (!TechTypeExtensions.FromString("IonPrism", out var ionPrism, false)) return new CraftNode("NRE");
        if (!TechTypeExtensions.FromString("Proto_PrecursorIngot", out var precursorIngot,false)) return new CraftNode("NRE");
        if (!AllInOneHandHeldFabricator.PrefabRegisters[treeType])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                new RecipeData(
                    new Ingredient(buildingBlock, 1),
                    new Ingredient(ionPrism, 1),
                    new Ingredient(precursorIngot, 1),
                    new Ingredient(TechType.PrecursorIonCrystalMatrix, 1)), TechType.None));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[treeType] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, treeType);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }

    public static List<CraftNode> RegisterCustomFabricatorUpgrades()
    {
        const string schemeId = "AIOHHFCraftUpgrade";
        List<CraftNode> craftNodes = new List<CraftNode>();
        foreach (CraftTree.Type treeType in AllInOneHandHeldFabricator.CustomFabricators.Keys)
        {
            if (AllInOneHandHeldFabricator.CustomFabricators[treeType].ToString().Equals("ProtoPrecursorFabricator")) continue;
            TechType customPrefab = AllInOneHandHeldFabricator.CustomFabricators[treeType];
            var craftTreeToYoink = CraftTree.GetTree(treeType);
            var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
            CraftTreeMethods.AddIconForNode(customPrefab, craftTreeTab, schemeId);
            CraftTreeMethods.AddLanguageForNode(customPrefab, craftTreeTab, schemeId);
            foreach (var craftNode in craftTreeToYoink.nodes)
            {
                CraftTreeMethods.AddIconForNode(craftTreeToYoink, craftNode, schemeId);
                craftTreeTab.AddNode(craftNode);
            }
            var language = Language.main.Get(customPrefab);
            if (AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Fabricator])
            {
                AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                    $"{language} Tree Upgrade", 
                    $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                    $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                    CraftDataHandler.GetModdedRecipeData(customPrefab), customPrefab));
            }
            AllInOneHandHeldFabricator.PrefabRegisters[treeType] = true;
            //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Fabricator);
            //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
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
        var language = Language.main.Get(TechType.Workbench);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Workbench])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.Workbench), TechType.Workbench));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Workbench] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Workbench);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterCyclopsFabricatorUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.CyclopsFabricator);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.Cyclops, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.CyclopsFabricator, craftTreeTab, schemeId);
        var language = Language.main.Get(TechType.Cyclops);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.CyclopsFabricator])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                new RecipeData(new Ingredient(TechType.Titanium, 3),
                    new Ingredient(TechType.Lithium, 2),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(TechType.ComputerChip, 1)), TechType.Cyclops));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.CyclopsFabricator] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.CyclopsFabricator);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
    
    public static CraftNode RegisterVehicleUpgradeConsoleUpgrade()
    {
        const string schemeId = "AIOHHFCraftTree";
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.SeamothUpgrades);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        AddIconForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId); 
        AddLanguageForNode(TechType.BaseUpgradeConsole, craftTreeTab, schemeId);
        var language = Language.main.Get(TechType.BaseUpgradeConsole);
        foreach (var craftNode in craftTreeToYoink.nodes)
        {
            AddIconForNode(craftTreeToYoink, craftNode, schemeId);
            craftTreeTab.AddNode(craftNode);
        }
        if (!AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.SeamothUpgrades])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.BaseUpgradeConsole), TechType.BaseUpgradeConsole));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.SeamothUpgrades] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.SeamothUpgrades);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
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
        if (!AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.MapRoom])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade", 
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." + 
                $" Gives the fabricator the related craftig tree.", craftTreeTab, 
                CraftDataHandler.GetRecipeData(TechType.BaseMapRoom), TechType.BaseMapRoom));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.MapRoom] = true;
        //AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.MapRoom);
        //AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
}