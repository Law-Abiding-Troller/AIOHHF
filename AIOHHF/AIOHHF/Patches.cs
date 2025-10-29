using System;
using AIOHHF.Items.Equipment;
using AIOHHF.Mono;
using HarmonyLib;
using UnityEngine;

namespace AIOHHF;


[HarmonyPatch(typeof(uGUI_CraftingMenu))]
public class uGUI_CraftingMenuPatches
{
    [HarmonyPatch(typeof(uGUI_CraftingMenu),nameof(uGUI_CraftingMenu.Filter), typeof(string))]
    [HarmonyPostfix]
    [HarmonyDebug]
    public static void Filter_Patches(uGUI_CraftingMenu __instance, string id, ref bool __result)
    {
        //Check if is my fabricator
        if (__instance._client is not HandHeldFabricator) return;
        //Cast to get my fabricator instance
        var instance = (HandHeldFabricator) __instance._client;
        //Search all items in my Fabricator's storage container that have a TechType
        foreach (var item in instance.gameObject.GetComponent<HandHeldPlayerTool>().storageContainer.container._items.Keys)
        {
            //Search every Upgrade for the TechType because they are the only things in the allowed tech field
            foreach (var prefab in AllInOneHandHeldFabricator.Upgrades)
            {
                //Is it one of the upgrades for the tree?
                if (!prefab.Tree.id.Equals(id)) return;
                //Is it my item?
                if (item == prefab.PrefabInfo.TechType)
                {
                    __result = false;
                    return;
                }
            }
        }
        __result = true;
    }
}