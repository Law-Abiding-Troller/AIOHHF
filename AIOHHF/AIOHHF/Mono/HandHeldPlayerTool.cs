using AIOHHF.Items.Equipment;


namespace AIOHHF.Mono;

public class HandHeldPlayerTool : PlayerTool
{
    public HandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
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
        fab.opened = true;
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
    }

    public override void OnDraw(Player p)
    {
        base.OnDraw(p);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
    }
    
}