using System;
using System.Collections;
using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using Nautilus.Utility;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UWE;
using Object = UnityEngine.Object;

namespace AIOHHF.Items;

public static class Fragments
{
    private static PrefabInfo[] _fragmentPIs = new PrefabInfo[3];
    private static CustomPrefab[] _fragmentCPs = new CustomPrefab[3];
    public static TechType FragmentsTechType;
    public static IEnumerator Initialize(WaitScreenHandler.WaitScreenTask task)
    {
        task.Status = "Initializing Fragments...";
        yield return null;
        var fragments = FragmentsTechType = EnumHandler.AddEntry<TechType>("AIOHHFFragment").Value;
        var multiplier = Plugin.ConfigFile.SpawnRate;
        var probability = 0.075f;
        if (multiplier < 0) probability /= 0-multiplier;
        else if (multiplier > 0) probability *= multiplier;
        var biomesToSpawnIn = new List<LootDistributionData.BiomeData>();
        foreach (BiomeType item in Enum.GetValues(typeof(BiomeType)))
        {
            if (!(item.AsString().Contains("Obsolete") || item.AsString().Contains("Unused")
                    || item.AsString().Contains("Wall") || item.AsString().Contains("Open")
                    || item.AsString().Contains("Ceiling") )
                && (item.AsString().Contains("Tech")
                    || item.AsString().Contains("EscapePod")
                    || item.AsString().Contains("Ship")
                    || item.AsString().Contains("Aurora")
                    || item.AsString().Contains("Crash")
                    || item.AsString().Contains("Supply")))
            {
                
                biomesToSpawnIn.Add(new LootDistributionData.BiomeData()
                {
                    biome = item,
                    probability = probability,
                    count = 1
                });
                yield return null;
            }
        }
        
        for (var i = 0; i < 3; i++)
        {
            _fragmentPIs[i] = new PrefabInfo("AIOHHFF" + i, "aiohhffragprefab" + i, fragments);
            var WEI = new WorldEntityInfo()
            {
                techType = fragments,
                classId = _fragmentPIs[i].ClassID,
                localScale = Vector3.one,
                slotType = EntitySlot.Type.Small,
                cellLevel = LargeWorldEntity.CellLevel.Global,
                prefabZUp = false
            };
            _fragmentCPs[i] = new CustomPrefab(_fragmentPIs[i]);
            _fragmentCPs[i].SetSpawns(WEI, biomesToSpawnIn.ToArray());
            var i1 = i;
            _fragmentCPs[i].SetGameObject(() =>
            {
                GameObject fragment =
                    Object.Instantiate(Plugin.Aiohhf.Bundle.LoadAsset<GameObject>("aiohhffragprefab"+(i1+1)));
                fragment.SetActive(false);
                PrefabUtils.AddBasicComponents(fragment, _fragmentPIs[i1].ClassID, _fragmentPIs[i1].TechType,
                    LargeWorldEntity.CellLevel.Global);
                MaterialUtils.ApplySNShaders(fragment);
                var rb = fragment.AddComponent<Rigidbody>();
                rb.mass = 5f;
                rb.useGravity = false;
                rb.isKinematic = true;
                var wf =  fragment.AddComponent<WorldForces>();
                wf.useRigidbody = rb;
                return fragment;
            });
            _fragmentCPs[i].CreateFragment(Plugin.Aiohhf.PrefabInfo.TechType, 3f,3, null, true, false);
            _fragmentCPs[i].Register();
        }
        yield return null;
    }
}
    