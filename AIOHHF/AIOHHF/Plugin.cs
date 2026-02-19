using System;
using System.Collections;
using System.IO;
using System.Reflection;
using AIOHHF.Items.Equipment;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;

namespace AIOHHF;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static readonly AllInOneHandHeldFabricator Aiohhf = new();

    public static Config ConfigFile;

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        ConfigFile = OptionsPanelHandler.RegisterModOptions<Config>();
        WaitScreenHandler.RegisterEarlyAsyncLoadTask(PluginInfo.PLUGIN_NAME, Aiohhf.RegisterPrefab, "Loading All-In-One Hand Held Fabricator");
        Preinitialize();
    }

    private static void Preinitialize()
    {
        Aiohhf.Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets", "aiohhfbundle"));
        Aiohhf.PrefabInfo = PrefabInfo.WithTechType("AIOHHF", "All-In-One Hand Held Fabricator", 
                "An All-In-One Hand Held Fabricator (AIOHHF). This fabricator has all other Fabricators! And is Hand Held(tm)!" +
                "\nUnfortunately, it holds no data of the Fabricator, you'll have to give it data. " +
                "Alterra is not responsible for data loss due to damage, lost, or destruction of this fabricator. " +
                "Energy consumption is the same as a normal Fabricator", "English", true)
            .WithIcon(Aiohhf.Bundle.LoadAsset<Sprite>("AIOHHF_Icon")).WithSizeInInventory(new Vector2int(2,2));
        Aiohhf.Prefab = new CustomPrefab(Aiohhf.PrefabInfo);
    }
}