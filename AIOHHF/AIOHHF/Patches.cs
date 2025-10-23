using AIOHHF.Items.Equipment;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;

namespace AIOHHF;

[HarmonyPatch(typeof(CustomPrefab))]
public class Patches
{
    [HarmonyPatch(nameof(CustomPrefab.Register))]
    [HarmonyPostfix]
    public static void CreateFabricator(CustomPrefab __instance)
    {
        if (__instance.Info.ClassID.Equals("AlienBuildingBlock") 
                    || __instance.Info.ClassID.Equals("IonPrism") 
                    || __instance.Info.ClassID.Equals("Proto_PrecursorIngot")) 
                    AllInOneHandHeldFabricator.RecipePrefabs.Add(__instance);
        if (!__instance.TryGetGadget<FabricatorGadget>(out var gadget)) return;
        AllInOneHandHeldFabricator.CustomFabricators.Add(gadget.CraftTreeType, __instance);
    }
}