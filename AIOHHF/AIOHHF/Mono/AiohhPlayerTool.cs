using UnityEngine;


namespace AIOHHF.Mono;

public class AiohhPlayerTool : PlayerTool
{
    public AioHandHeldFabricator fab;
    public PowerRelay relay;
    public HandHeldBatterySource battery;
    public StorageContainer storageContainer;
    private double _counter;
    public Transform leftHand;
    public Transform rightHand;
    public override string animToolName => "seaglide";

    public override void Awake()
    {
        //socket = Socket.Camera;
        fab = gameObject.GetComponent<AioHandHeldFabricator>();
        relay = gameObject.GetComponent<HandHeldRelay>();
        fab.powerRelay = relay;
        battery = gameObject.GetComponent<HandHeldBatterySource>();
        storageContainer = gameObject.GetComponent<StorageContainer>();
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
        /*_hands = Plugin.Aiohhf.Bundle.LoadAsset<GameObject>("Hands");
        Instantiate(_hands, Player.main.gameObject.transform);
        leftHand = _hands.FindChild("left_hand").transform;
        rightHand = _hands.FindChild("right_hand").transform;*/
        base.Awake();
    }

    public void Start()
    {
        ikAimRightArm = true;
        ikAimLeftArm = true;
        savedIkAimRightArm = true;
        savedIkAimLeftArm = true;
    }

    public override bool OnRightHandDown()
    {
        fab.opened = true;
        fab.animator.SetBool(AnimatorHashID.open_fabricator, true);
        _counter = 0f;
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
            if (_counter >= 7f)
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
        if (leftHand != null && rightHand != null)
        {
            //leftHandIKTarget = leftHand;
            //rightHandIKTarget = rightHand;
        }
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