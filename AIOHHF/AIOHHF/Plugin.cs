using System;
using System.Collections;
using System.IO;
using System.Reflection;
using AIOHHF.Items.Equipment;
using AIOHHF.Mono;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
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
    
    public static EquipmentType EquipmentType = EnumHandler.AddEntry<EquipmentType>("AIOHHF").Value;
    
    public static GameInput.Button TryPickUpButton =  EnumHandler.AddEntry<GameInput.Button>("AIOHHFTryPickUp")
        .CreateInput("","",Language.main.GetCurrentLanguage())
        .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.G)
        .WithCategory("AIOHHF");

    private void Awake()
    {
        // set project-scoped logger instance
        Logger = base.Logger;

        // register harmony patches, if there are any
        Harmony.CreateAndPatchAll(Assembly, $"{PluginInfo.PLUGIN_GUID}");
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        ConfigFile = OptionsPanelHandler.RegisterModOptions<Config>();
        LanguageHandler.RegisterLocalizationFolder();
        WaitScreenHandler.RegisterEarlyAsyncLoadTask(PluginInfo.PLUGIN_NAME, Aiohhf.RegisterPrefab, "Loading All-In-One Hand Held Fabricator");
        WaitScreenHandler.RegisterLateAsyncLoadTask(PluginInfo.PLUGIN_NAME, Aiohhf.LateRegister, "Registering Modded Fabricators");
        Preinitialize();
    }

    private static void Preinitialize()
    {
        Aiohhf.Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location), "Assets", "aiohhfbundle"));
        Aiohhf.PrefabInfo = PrefabInfo.WithTechType("AIOHHF", null, null, Language.main.GetCurrentLanguage())
            .WithIcon(Aiohhf.Bundle.LoadAsset<Sprite>("AIOHHF_Icon")).WithSizeInInventory(new Vector2int(2,2));
        Aiohhf.Prefab = new CustomPrefab(Aiohhf.PrefabInfo);
        var slots = new string[4];
        for (var i = 0; i < 4; i++)
        {
            var str = EquipmentType.ToString() + (i + 1);
            slots[i] = str; 
            Equipment.slotMapping.Add(str, EquipmentType);
        }
        DataTypes.Slots.Add(new DataTypes(slots,Aiohhf.PrefabInfo.TechType));
    }
}