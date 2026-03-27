using System.Collections.Generic;
using System.Linq;
using AIOHHF.Items.Upgrades;
using Nautilus.Crafting;
using UnityEngine;

namespace AIOHHF.Mono;

public static class Extentions
{
    /// <summary>
    /// Copies a transform positional data to another transform
    /// </summary>
    /// <param name="source">The transform to be copied</param>
    /// <param name="target">The transform to copy to</param>
    public static void CopyTransformPosition(this Transform source, Transform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.eulerAngles = source.eulerAngles;
    }
    /// <summary>
    /// Copies a transform's local positional data to another transform
    /// </summary>
    /// <param name="source">The transform to be copied</param>
    /// <param name="target">The transform to copy to</param>
    public static void CopyTransformLocalPosition(this Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localEulerAngles = source.localEulerAngles;
    }
    public static void CopyTransformToLocalPosition(this Transform source, Transform target)
    {
        target.localPosition = source.position;
        target.localRotation = source.rotation;
        target.localEulerAngles = source.eulerAngles;
    }
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

    public static void EnsureValidClassID(this string str)
    {
        str = str.Replace(" ", "").Replace("_", "").Replace("/", "")
            .Replace("-", "").Replace("+", "").Replace("=", "")
            .Replace("[", "").Replace("]", "").Replace(",", "")
            .Replace("{", "").Replace("}", "")
            .Replace("'", "").Replace("|", "");
    }
}