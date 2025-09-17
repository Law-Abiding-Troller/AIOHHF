using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using UnityEngine;

namespace AIOHHF;

public class Async //Implementation 1
{
    public static Dictionary<CraftTree.Type, TechType> CrafterTechTypes = new();
    public static IEnumerator GetTechTypeForCraftersAsync(WaitScreenHandler.WaitScreenTask task)
    {
        for (int i = 10024; i < Enum.GetValues(typeof(TechType)).Length; i++)
        {
            TechType techType = (TechType)i;
            task.Status = $"Getting prefab for {techType}";
            CoroutineTask<GameObject> prefab = CraftData.GetPrefabForTechTypeAsync(techType);
            if (prefab == null) continue;
            yield return prefab;
            GameObject possibleCrafter = prefab.GetResult();
            if (possibleCrafter == null) continue;
            if (!possibleCrafter.TryGetComponent<GhostCrafter>(out var crafter)) continue;
            CrafterTechTypes.Add(crafter.craftTree, techType);
        }
    }
}