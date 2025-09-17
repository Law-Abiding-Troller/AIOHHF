using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Handlers;
using UnityEngine;

namespace AIOHHF;

public class Async
{
    public static Dictionary<CraftTree.Type, TechType> CrafterTechTypes = new();
    public static IEnumerator GetTechTypeForCrafters(WaitScreenHandler.WaitScreenTask task)
    {
        foreach (TechType techType in Enum.GetValues(typeof(TechType)))
        {
            CoroutineTask<GameObject> prefab = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return prefab;
            GameObject possibleCrafter = prefab.GetResult();
            var crafter = possibleCrafter.GetComponent<GhostCrafter>();
            if (crafter == null) continue;
            CrafterTechTypes.Add(crafter.craftTree, techType);
        }
    }
}