using System;
using System.Collections;
using System.IO;
using System.Reflection;
using AIOHHF.Items.Equipment;
using AIOHHF.Mono;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Utility.ModMessages;
using UnityEngine;

namespace AIOHHF;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency("sn.easycraft.mod", BepInDependency.DependencyFlags.SoftDependency)]
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
        .WithControllerBinding(GameInputHandler.Paths.Gamepad.LeftBumper)
        .WithCategory("AIOHHF");

    private void Awake()
    {
        LanguageHandler.RegisterLocalizationFolder();
        // set project-scoped logger instance
        Logger = base.Logger;

        // register harmony patches, if there are any
        PatchAll();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        ConfigFile = OptionsPanelHandler.RegisterModOptions<Config>();
        ModMessageSystem.SendGlobal("FindMyUpdates", "https://raw.githubusercontent.com/Law-Abiding-Developer/AIOHHF/refs/heads/main/AIOHHF/AIOHHF/Version.json");
        
        WaitScreenHandler.RegisterEarlyAsyncLoadTask(PluginInfo.PLUGIN_NAME, Aiohhf.RegisterPrefab, "Loading All-In-One Hand Held Fabricator");
        WaitScreenHandler.RegisterLateAsyncLoadTask(PluginInfo.PLUGIN_NAME, Aiohhf.LateRegister, "Registering Modded Fabricators");
        Preinitialize();
    }

    private static void Preinitialize()
    {
        Aiohhf.Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.Location),
            "Assets", "aiohhfbundle"));
        var icon = Aiohhf.Bundle.LoadAsset<Sprite>("AIOHHF_Icon");
        CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "AIOHHFTab",
            Language.main.Get("TabName"), icon, "Personal");
        Aiohhf.PrefabInfo = PrefabInfo.WithTechType("AIOHHF", null, 
                null, Language.main.GetCurrentLanguage())
            .WithIcon(icon).WithSizeInInventory(new Vector2int(2,2));
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

    public static void PatchAll()
    {
        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(uGUI_CraftingMenu));
        harmony.PatchAll(typeof(GhostCrafter));
        harmony.PatchAll(typeof(uGUI_Equipment));
        if (!Chainloader.PluginInfos.TryGetValue("sn.easycraft.mod", out var mod) || mod == null || mod.Instance == null) return;
    }
}