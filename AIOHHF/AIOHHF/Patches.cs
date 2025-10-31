using System;
using System.CodeDom;
using AIOHHF.Items.Equipment;
using AIOHHF.Mono;
using HarmonyLib;
using UnityEngine;

namespace AIOHHF;


[HarmonyPatch(typeof(uGUI_CraftingMenu))]
public class uGUI_CraftingMenuPatches
{
    /*[HarmonyPatch(typeof(uGUI_CraftingMenu),nameof(uGUI_CraftingMenu.Filter), typeof(string))]
    [HarmonyPostfix]
    [HarmonyDebug]
    public static void Filter_Patches(uGUI_CraftingMenu __instance, string id, ref bool __result)
    {
        //Check if is my fabricator
        var instance = __instance.gameObject.GetComponent<HandHeldFabricator>();
        if (instance == null) return;
        //Search all items in my Fabricator's storage container that have a TechType
        foreach (TechType item in instance.gameObject.GetComponent<StorageContainer>().container._items.Keys)
        {
            //Check if the TechType has a node attached to it through the Upgrade prefabs
            if (!AllInOneHandHeldFabricator.Nodes.TryGetValue(item, out var node)) continue;
            //Check if the node.id is the current id. If not, continue
            if (!node.id.Equals(id)) continue;
            //At this point, it is, so make sure it appears.
            __result = true;
            
            //Scraped
            /*Search every Upgrade for the TechType because they are the only things in the allowed tech field
            foreach (var prefab in AllInOneHandHeldFabricator.Upgrades)
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
            }
        }
        //No checks were successful at this point, filter it out of the tree
        __result = false;
    }*/
}