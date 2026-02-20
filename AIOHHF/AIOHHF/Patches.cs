
using AIOHHF.Items.Equipment;
using AIOHHF.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AIOHHF;


[HarmonyPatch(typeof(uGUI_CraftingMenu))]
public class uGUI_CraftingMenuPatches
{
    [HarmonyPatch(typeof(uGUI_CraftingMenu),nameof(uGUI_CraftingMenu.Filter), typeof(string))]
    [HarmonyPostfix]
    public static void Filter_Patches(uGUI_CraftingMenu __instance, string id, ref bool __result)
    {
        //Check if is my fabricator, if so, cast.
        if (__instance._client is not AioHandHeldFabricator instance) return;
        //For checking
        bool isSuperTab = false;
        //Search through each Super Tab to see if the ID matches the ID of the Super Tab
        foreach (var item in AllInOneHandHeldFabricator.Trees)
        {
            //If true, set isSuperTab to True
            if (item.id.Equals(id)) isSuperTab = true;
        }
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
    public static bool Open_Patches(uGUI_CraftingMenu __instance, ITreeActionReceiver receiver)
    {
        if (receiver is AioHandHeldFabricator fab && fab.gameObject.GetComponent<StorageContainer>().IsEmpty())
        {
            ErrorMessage.AddWarning("Lacking data to form a craft tree!");
            return false;
        }
        __instance._client = receiver;
        return true;
    }
}

[HarmonyPatch(typeof(GhostCrafter))]
public class GhostCrafterPatches
{
    [HarmonyPatch(nameof(GhostCrafter.OnHandHover))]
    [HarmonyPrefix]
    public static bool OnHandHover_Patches(GhostCrafter __instance, GUIHand hand)
    {
        if (!__instance.gameObject.TryGetComponent<AiohhPlayerTool>(out var pt)) 
            return true;
        pt.pickupable.OnHandHover(hand);
        return false;
    }

    [HarmonyPatch(nameof(GhostCrafter.OnHandClick))]
    [HarmonyPrefix]
    public static bool OnHandClick_Patches(GhostCrafter __instance, GUIHand hand)
    {
        if (!__instance.gameObject
                .TryGetComponent<AiohhPlayerTool>(out var pt)) 
            return true;
        pt.pickupable.OnHandClick(hand);
        return false;
    }
}
[HarmonyPatch(typeof(uGUI_Equipment))]
public class uGUI_EquipmentPatches
{
    
    [HarmonyPatch(nameof(uGUI_Equipment.Awake))]
    [HarmonyPrefix, HarmonyDebug]
    public static void Awake_Patches(uGUI_Equipment __instance)
    {
        foreach (var slotArray in DataTypes.Slots)
        {
            CloneSlots(__instance, slotArray);
        }
    }
    #nullable enable
    public static uGUI_EquipmentSlot? CloneSlots(uGUI_Equipment equipment, DataTypes moddedUpgradeConsoleInput,
        string copyTarget = "SeamothModule", string? imageTarget = "Seamoth", Vector3[]? slotPositions = null,
        float scale = 1)
    {
        var slots = moddedUpgradeConsoleInput.Strings;
        Plugin.Logger.LogInfo("Cloning slots...");
        if (slots.Length == 0) return null;

        uGUI_EquipmentSlot slot = CloneSlot(equipment, $"{copyTarget}1", slots[0], scale);
        if (imageTarget != null)
        {
            var image = slot.transform.Find(imageTarget).GetComponent<Image>();
            image.sprite = SpriteManager.Get(moddedUpgradeConsoleInput.TechType);
            image.SetNativeSize();
            image.color = new Color(0, image.color.g/1.4f, image.color.b, 0.25f);
            image.rectTransform.localScale = Vector3.one;
        }

        if (slotPositions != null)
        {
            slot.transform.localPosition = slotPositions[0];
        }

        for (int i = 1; i < slots.Length; i++)
        {
            var clonedSlot = CloneSlot(equipment, $"{copyTarget}{Mathf.Min(4, i + 1)}", slots[i], scale);
            if (slotPositions != null)
            {
                clonedSlot.transform.localPosition = slotPositions[i];
            }
        }
        return slot;
    }
    #nullable disable
    private static uGUI_EquipmentSlot CloneSlot(uGUI_Equipment equipmentMenu, string childName, string newSlotName, float scale)
    {
        Transform newSlot = Object.Instantiate(equipmentMenu.transform.Find(childName), equipmentMenu.transform);
        newSlot.transform.localScale = Vector3.one * scale;
        newSlot.name = newSlotName;
        uGUI_EquipmentSlot equipmentSlot = newSlot.GetComponent<uGUI_EquipmentSlot>();
        equipmentSlot.slot = newSlotName;
        return equipmentSlot;
    }
}