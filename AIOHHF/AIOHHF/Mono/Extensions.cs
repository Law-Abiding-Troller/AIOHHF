using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AIOHHF.Mono;

public static class Extensions
{
    public static void Recover(this Equipment equipment, Transform root, IEnumerable<string> slots)
    {
        foreach (Transform child in root)
        {
            if (!child.TryGetComponent(out Pickupable item))
            {
                Plugin.Logger.LogWarning($"[{equipment._label}] Found non-item ({item.gameObject}), destroying...");
                GameObject.Destroy(item.gameObject);
                continue;
            }

            if (slots.All(slot => equipment.GetItemInSlot(slot) != null))
            {
                Plugin.Logger.LogWarning($"[{equipment._label}] Found extra item ({item.gameObject}), destroying...");
                GameObject.Destroy(item.gameObject);
                continue;
            }

            InventoryItem inventoryItem = new InventoryItem(item);
            if (!slots.Any(slot => equipment.AddItem(slot, inventoryItem)))
            {
                Plugin.Logger.LogWarning($"[{equipment._label}] Found invalid item ({item.gameObject}), destroying...");
                GameObject.Destroy(item.gameObject);
            }
        }
    }
}