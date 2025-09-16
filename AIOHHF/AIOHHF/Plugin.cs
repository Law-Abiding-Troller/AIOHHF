using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using AIOHHF.Items.Equipment;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;

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
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        WaitScreenHandler.RegisterLateLoadTask(PluginInfo.PLUGIN_NAME, CreateCraftTree, "Loading All-In-One Hand Held Fabricator");
        SaveUtils.RegisterOnQuitEvent(DeregisterPrefabs);
    }

    public static void CreateCraftTree(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Creating AIOHHF PrefabInfo and TechType";
        Plugin.Logger.LogDebug(task.Status);
        Items.Equipment.AIOHHF.AIOHHFPrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held!" +
                "\nEnergy consumption is the same as a normal Fabricator")
            .WithIcon(SpriteManager.Get(TechType.Fabricator)).WithSizeInInventory(new Vector2int(2,2));
        task.Status = "Initializing AIOHHF Prefab";
        Plugin.Logger.LogDebug(task.Status);
        Items.Equipment.AIOHHF.AIOHHFPrefab = new CustomPrefab(Items.Equipment.AIOHHF.AIOHHFPrefabInfo);
        Items.Equipment.AIOHHF.AIOHHFFabricator = Items.Equipment.AIOHHF.AIOHHFPrefab.CreateFabricator(out Items.Equipment.AIOHHF.AIOHHFTreeType);
        int secondaryiterator = 0;
        int thirditerator = 0;
        foreach (CraftTree.Type treeType in Enum.GetValues(typeof(CraftTree.Type)))
        {
            Logger.LogDebug(treeType.ToString()+". we happy cause the enum exists now");
            if (treeType == CraftTree.Type.Constructor || treeType == CraftTree.Type.None ||
                treeType == CraftTree.Type.Unused1 || treeType == CraftTree.Type.Unused2 || treeType == CraftTree.Type.Rocket || treeType == Items.Equipment.AIOHHF.AIOHHFTreeType) continue;
            task.Status =
                $"Creating AIOHHF Tree\nTree: {CraftTree.GetTree(treeType).id}\nIteration: {secondaryiterator}";
            Logger.LogDebug(task.Status);
            //Items.Equipment.AIOHHF.AIOHHFFabricator.AddTabNode(CraftTree.GetTree(treeType).id + "AIOHHFTab",
                //CraftTree.GetTree(treeType).id,
                //SpriteManager.Get(TechType.Fabricator));
            
            foreach (CraftNode node in CraftTree.GetTree(treeType).nodes)
            {
                task.Status = $"Creating AIOHHF Tree\nCurrent Tree: {CraftTree.GetTree(Items.Equipment.AIOHHF.AIOHHFTreeType)}\nAdding Tree: {CraftTree.GetTree(treeType).id}\nIteration: {secondaryiterator}\nNode Iteration: {thirditerator}\nNode Added: {node.id}";
                            Logger.LogDebug(task.Status);
                CraftTree.GetTree(Items.Equipment.AIOHHF.AIOHHFTreeType).nodes.AddNode(node);
                thirditerator++;
            }
            secondaryiterator++;
            thirditerator++;
        }
        Items.Equipment.AIOHHF.RegisterPrefab();
        /*task.Status = "Creating AIOHHF Tree";
        foreach (var tech in CraftTree.craftableTech)
        {
            Items.Equipment.AIOHHF.AIOHHFFabricator.AddCraftNode(tech);
        }*/
    }

    public static void DeregisterPrefabs()
    {
        Items.Equipment.AIOHHF.AIOHHFPrefab.Unregister();
    }
}