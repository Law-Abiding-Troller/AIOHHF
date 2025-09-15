using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using AIOHHF.Items.Equipment;
using Nautilus.Handlers;

namespace AIOHHF;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        WaitScreenHandler.RegisterLateLoadTask("AIOHHF", CreateCraftTree, "Loading All-In-One Hand Held Fabricator");
        Items.Equipment.AIOHHF.RegisterPrefab();
    }

    public static void CreateCraftTree(WaitScreenHandler.WaitScreenTask task)
    {
        int secondaryiterator = 0;
        foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
        {
            if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None || treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2) continue;
            task.Status = $"Creating AIOHHF Tree\nTree: {CraftTree.GetTree(treeType).id}\nIteration: {secondaryiterator}";
            Plugin.Logger.LogDebug(task.Status);
           Items.Equipment.AIOHHF.AIOHHFFabricator.AddTabNode(CraftTree.GetTree(treeType).id + "AIOHHFTab",
                CraftTree.GetTree(treeType).id,
                SpriteManager.Get(TechType.Fabricator));
            CraftTree.GetTree(Items.Equipment.AIOHHF.AIOHHFTreeType)
                .nodes[secondaryiterator]
                .AddNode(CraftTree.GetTree(treeType).nodes);
            secondaryiterator++;
        }
        

    }
}

public class PluginInfo
{
    public const string PLUGIN_GUID = "com.lac aiohhf";
    public const string PLUGIN_NAME =  "AIOHHF";
    public const string PLUGIN_VERSION = "v0.0.1";
}