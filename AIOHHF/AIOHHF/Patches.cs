using System;
using System.CodeDom;
using System.Collections.Generic;
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
        //For checking
        bool isSuperTab = false;
        //Search through each Super Tab to see if the ID matches the ID of the Super Tab
        foreach (var item in AllInOneHandHeldFabricator.Trees)
        {
            //If true, set isSuperTab to True
            if (item.id.Equals(id)) isSuperTab = true;
        }
        //Check if is my fabricator, if so, cast.
        if (__instance._client is not HandHeldFabricator instance) return;
        //Set the default case to false so long as is it a Super Tab so
        //that it filters everything but what the foreach loop finds
        if (isSuperTab) __result = false;
        //Search all items in my Fabricator's storage container that have a TechType
        foreach (TechType item in instance.gameObject.GetComponent<StorageContainer>().container._items.Keys)
        {
            //Check if the TechType has a node attached to it through the
            //Upgrade prefabs
            if (!AllInOneHandHeldFabricator.Nodes.TryGetValue(item, out var node)) continue;
            //Check if the node.id is the current id. If not, continue
            if (!node.id.Equals(id)) continue;
            //At this point, it is, so make sure it appears.
            __result = true;
            
            //Scraped
            //Search every Upgrade for the TechType because they are the only things in the allowed tech field
            /*foreach (var prefab in AllInOneHandHeldFabricator.Upgrades)
            {
                //Is it one of the upgrades for the tree?
                if (!prefab.Tree.id.Equals(id)) return;
                //Is it my item?
                if (item == prefab.PrefabInfo.TechType && id.Equals(prefab.Tree.id))
                {
                    //Don't filter it
                    __result = false;
                    return;
                }
            }*/
        }
    }

    [HarmonyPatch(nameof(uGUI_CraftingMenu.Open))]
    [HarmonyPrefix]
    public static void Open_Patches(uGUI_CraftingMenu __instance, ITreeActionReceiver receiver)
    {
        __instance._client = receiver;
    }
}