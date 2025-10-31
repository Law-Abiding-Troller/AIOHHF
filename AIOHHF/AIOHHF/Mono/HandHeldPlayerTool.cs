using AIOHHF.Items.Equipment;
using UnityEngine;


namespace AIOHHF.Mono;

public class HandHeldPlayerTool : PlayerTool
{
    public HandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
    private double _counter = 0;
    public override void Awake()
    {
        fab = gameObject.GetComponent<HandHeldFabricator>();
        relay = gameObject.GetComponent<PowerRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<StorageContainer>();
        battery.connectedRelay = relay;
        relay.AddInboundPower(battery);
    }
    public override bool OnRightHandDown()
    {
        //Set the open bool on the fabricator
        fab.opened = true;
        //Physically open the fabricator
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        //Set close wait timer to 0
        _counter = 0f;
        //Search through every item in the storage container for any tree
        foreach (var item in storageContainer.container._items.Keys)
        {
            //If it is equal to none, skip it
            if (item == TechType.None) continue;
            //If the TechType is not any of my upgrades, skip it
            if (!AllInOneHandHeldFabricator.Nodes.TryGetValue(item, out var node)) continue;
            //Add to collection for active trees
            AllInOneHandHeldFabricator.Trees.Add(node);
        }
        //Open the fabricator UI
        uGUI.main.craftingMenu.Open(Plugin.Aiohhf.TreeType, fab);
        return true;
    }

    public override bool OnAltDown()
    {
        if (!storageContainer.open && storageContainer != null && storageContainer.container != null)
        {
            storageContainer.container._label =  "ALL IN ONE HAND HELD FABRICATOR";
            storageContainer.Open();
        }

        return true;
    }

    public void Update()
    {
        gameObject.transform.localScale = Plugin.Aiohhf.PostScaleValue;
        _counter += Time.deltaTime;
        if (_counter >= 7f
            && !uGUI.main.craftingMenu.isActiveAndEnabled)
        {
            fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
            _counter = 0;
        }
        else if (uGUI.main.craftingMenu.isActiveAndEnabled)
        {
            fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
            _counter = 0;
        }

        if (fab.crafterLogic.inProgress && !fab.animator.GetBool(AnimatorHashID.open_fabricator))
        {
            fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        }
    }

    public override void OnDraw(Player p)
    {
        base.OnDraw(p);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        _counter = 0;
    }

    public override void OnHolster()
    {
        base.OnHolster();
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
        _counter = 0;
    }
}