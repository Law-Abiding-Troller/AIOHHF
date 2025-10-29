using System.Collections.Generic;
using AIOHHF.Items.Equipment;

namespace AIOHHF.Mono;

public class HandHeldFabricator : Fabricator
{
    
    public override void OnOpenedChanged(bool opened)
    {
        AllInOneHandHeldFabricator.ActiveNodes = new List<CraftNode>();
        foreach (TechType item in gameObject.GetComponent<StorageContainer>().container._items.Keys)
        {
            CraftNode node = new CraftNode("NRE");
            if (item == TechType.None) continue;
            foreach (var treeType in AllInOneHandHeldFabricator.Upgrades)
            {
                if (item == treeType.PrefabInfo.TechType)
                {
                    node = treeType.Tree;
                }
            }
            AllInOneHandHeldFabricator.ActiveNodes.Add(node);
        }
        base.OnOpenedChanged(opened);
    }
}