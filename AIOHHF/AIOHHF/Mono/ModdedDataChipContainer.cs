using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AIOHHF.Mono;

public class ModdedDataChipContainer : MonoBehaviour
{
    [FormerlySerializedAs("_slots")] [SerializeField]
    private string[] slots;
    public Equipment equipment;
    private GameObject _child;

    public void Start()
    {
        if (equipment == null) InitializeEquipment();
    }

    public void InitializeEquipment()
    {
        var techType = CraftData.GetTechType(gameObject);
        _child = gameObject.FindChild(DataTypes.ChildObjects[techType]);
        equipment = new Equipment(gameObject, _child.transform);
        slots = DataTypes.Equipment[techType];
        equipment._label = DataTypes.Labels[techType];
        equipment.AddSlots(slots);
        equipment.Recover(_child.transform, slots);
    }

    public void OpenPDA()
    {
        if (equipment == null) return;
        Inventory.main.SetUsedStorage(equipment);
        if (Player.main.pda.Open(PDATab.Inventory)) return;
        Player.main.pda.Close();
    }

    public bool IsEmpty()
    {
        if (equipment == null) return true;
        if (equipment.equipment == null) return true;
        if (equipment.equipment.Count <= 0) return true;
        if (equipment.equipment.Values.Count <= 0) return true;
        var allNull = true;
        foreach (var e in equipment.equipment.Values)
        {
            if (e != null) allNull = false;
        }
        return allNull;
    }

    private PrefabIdentifier GetPrefabIdentifier()
    {
        GameObject go = gameObject;
        while (go.GetComponent<PrefabIdentifier>() == null)
        {
            go = go.transform.parent.gameObject;
        }
        return go.GetComponent<PrefabIdentifier>();
    }
}

public class DataTypes
{
    public static readonly List<DataTypes> Slots = new();
    public static readonly Dictionary<TechType, string[]> Equipment = new();
    public static readonly Dictionary<TechType, string> Labels = new();
    public static readonly Dictionary<TechType, string> ChildObjects = new();
    public string[] Strings;
    public TechType TechType;
    public DataTypes(string[] strings, TechType techType)
    {
        Strings = strings;
        TechType = techType;
    }
}