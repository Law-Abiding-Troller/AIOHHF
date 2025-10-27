using AIOHHF.Items.Equipment;
using AIOHHF.Items.Upgrades;
using BepInEx;
using Nautilus.Handlers;

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
        var craftTreeToYoink = CraftTree.GetTree(CraftTree.Type.Fabricator);
        var craftTreeTab = new CraftNode(craftTreeToYoink.id, TreeAction.Expand);
        var language = Language.main.Get(TechType.Fabricator);
        if (AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Fabricator])
        {
            AllInOneHandHeldFabricator.Upgrades.Add(new UpgradesPrefabs($"{language}Upgrade",
                $"{language} Tree Upgrade",
                $"{language} Tree Upgrade for the All-In-One Hand Held Fabricator." +
                $" Gives the fabricator the related craftig tree.", craftTreeTab, CraftDataHandler.GetRecipeData(TechType.Fabricator), TechType.Fabricator));
        }
        AllInOneHandHeldFabricator.PrefabRegisters[CraftTree.Type.Fabricator] = true;
        AllInOneHandHeldFabricator.Fabricators.Add(craftTreeTab, CraftTree.Type.Fabricator);
        AllInOneHandHeldFabricator.Trees.Add(craftTreeTab);
        return craftTreeTab;
    }
}