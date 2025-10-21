using AIOHHF.Items.Equipment;
using HarmonyLib;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;

namespace AIOHHF;

[HarmonyPatch(typeof(GadgetExtensions))]
public class Patches
{
    [HarmonyPatch(nameof(GadgetExtensions.CreateFabricator))]
    [HarmonyPostfix]
    public static void CreateFabricator(ICustomPrefab customPrefab)
    {
        if (!customPrefab.TryGetGadget<FabricatorGadget>(out var gadget))
        {
            customPrefab.CreateFabricator(out var treeType);
            AllInOneHandHeldFabricator.CustomFabricators.Add(treeType, customPrefab);
        }
        else
        {
            AllInOneHandHeldFabricator.CustomFabricators.Add(gadget.CraftTreeType, customPrefab);
        }
    }
}