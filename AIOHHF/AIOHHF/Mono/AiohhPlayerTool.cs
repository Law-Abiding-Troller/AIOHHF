using UnityEngine;


namespace AIOHHF.Mono;

public class AiohhPlayerTool : PlayerTool
{
    public AioHandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public ModdedDataChipContainer storageContainer;
    public override string animToolName => "seaglide";

    public override void Awake()
    {
        //socket = Socket.Camera;
        fab = gameObject.GetComponent<AioHandHeldFabricator>();
        relay = gameObject.GetComponent<HandHeldRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<ModdedDataChipContainer>();
        pickupable = gameObject.GetComponent<Pickupable>();
        pickupable.droppedEvent.AddHandler(pickupable, parms =>
        {
            parms.gameObject.FindChild("collision").SetActive(true);
        });
        pickupable.pickedUpEvent.AddHandler(pickupable, parms =>
        {
            parms.gameObject.FindChild("collision").SetActive(false);
        });
        battery.connectedRelay = relay;
        relay.AddInboundPower(battery);
        base.Awake();
    }

    public void Start()
    {
        ikAimRightArm = true;
        ikAimLeftArm = true;
        savedIkAimRightArm = true;
        savedIkAimLeftArm = true;
        fab.crafterLogic.TryPickup();
    }

    public override bool OnRightHandDown()
    {
        fab.opened = true;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        uGUI.main.craftingMenu.Open(Plugin.Aiohhf.TreeType, fab);
        fab.crafterLogic.TryPickup();
        return true;
    }

    public override bool OnAltDown()
    {
        fab.crafterLogic.TryPickup();
        storageContainer.OpenPDA();
        
        return true;
    }

    public void Update()
    {
            gameObject.transform.localScale = Plugin.Aiohhf.PostScaleValue;
        
        
            if (uGUI.main.craftingMenu.isActiveAndEnabled)
            {
                fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
            }

            if (fab.crafterLogic.inProgress)
            {
                fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
            }

            if (Player.main.IsFreeToInteract() && GameInput.GetButtonDown(Plugin.TryPickUpButton))
            {
                fab.crafterLogic.TryPickup();
            }
    }

    public override void OnDraw(Player p)
    {
        fab.crafterLogic.TryPickup();
        base.OnDraw(p);
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        
    }

    public override void OnHolster()
    {
        fab.crafterLogic.TryPickup();
        base.OnHolster();
        if (fab.animator == null) return;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, false);
    }

    public override string GetCustomUseText()
    {
        return $"{Language.main.Get("OpenFabText")} ({GameInput.FormatButton(GameInput.Button.RightHand)}), " +
               $"{Language.main.Get("OpenDataText")} ({GameInput.FormatButton(GameInput.Button.AltTool)}), "
               + $"{Language.main.Get("OptionAIOHHFTryPickUp")} ({GameInput.FormatButton(Plugin.TryPickUpButton)})";
    }
}