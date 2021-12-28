using DG.Tweening;
using TMPro;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In Thi4f, the lockpicking game involves moving lockpicks to the correct angle, then pressing a key to lock the tumbler in place and move to the next
public class Thief4LockpickGame : LockpickGame
{
    public RectTransform Lockpick1;
    //The lockpick that actually moves. The other one is Lockpick 1, but doesn't move at all so it isn't needed here
    public RectTransform Lockpick2;

    //We keep track of our mouse position so that we can change the angle as the player moves the mouse
    Vector3 StartMousePos;
    Vector3 LastMousePos;

    //A reference to our player object so that we can lock it in place while the game is going on
    Player ThePlayer;

    //This is generated with every tumbler - this is the success area the player is trying to find
    float targetAngle;

    //The success range gives the player a small range of degrees around the target angle which are considered "correct". 
    //without this the player would have to be right on, which is a lot to ask with imprecise controls
    public float TargetAngleSuccessRange = 5.0f;


    //When the player enters the range we set this to true
    bool InSuccessRange = false;

    //Keeps track of the current time remaining on the hold
    //float SuccessTimer;

    int CurrentTumbler = 0;

    //The tumbler objects - see the Thief4Tumbler class for more info
    public Thief4Tumbler[] Tumblers;

    //We set up the positions that the pointer should move to by hand
    public Vector2[] PointerPositions;

    //The pointer's transform for moving it around
    public RectTransform Pointer;

    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;
    //This is for debug feedback when the player is close to the target or on the target
    //public TextMeshProUGUI StatusText;
    //We tell the player their current angle and the target angle in the museum to give a more accurate view into the game's workings
    public TextMeshProUGUI TargetAngleText;
    public TextMeshProUGUI CurrentAngleText;

    //Called when the player starts the lockpicking game. It sets up our initial values
    public override void BeginLockpicking(Player player)
    {
        //For now, just MKB
        LastMousePos = StartMousePos = Input.mousePosition;

        GamePanel.SetActive(true);

        player.FreezePlayer();

        ThePlayer = player;

        //Get a random target angle for the first tumbler
        targetAngle = Random.Range(0, 360);

        InSuccessRange = false;

        CurrentTumbler = 0;

        UpdateTumblers();
        TargetAngleText.text = targetAngle.ToString("F0");
    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSuccess()
    {
        EndLockpicking();
        OpenLock();
    }

    //This figures out the state of all the tumblers and makes the graphics match
    void UpdateTumblers()
    {
        if (CurrentTumbler < 3)
        {
            for (int i = 0; i < 3; i++)
            {
                //If we've already solved this tumbler, then make it smaller and fully opaque
                if (i < CurrentTumbler)
                {
                    //I hate this pattern but it's the way Unity does things
                    Color c = Tumblers[i].InnerCircle.color;
                    c.a = 1;
                    Tumblers[i].InnerCircle.color = c;
                    Tumblers[i].GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                //Otherwise, it's either the current one or a future one, set it to be transparent
                else
                {
                    Color c = Tumblers[i].InnerCircle.color;
                    c.a = 0;
                    Tumblers[i].InnerCircle.color = c;
                }
            }

            //If it is the current one, make it big, move the pointer to point at it, and choose a random target for it
            Tumblers[CurrentTumbler].GetComponent<RectTransform>().localScale = Vector3.one;
            Pointer.anchoredPosition = PointerPositions[CurrentTumbler];
            targetAngle = Random.Range(0, 360);
            TargetAngleText.text = targetAngle.ToString("F0");
        }
        else
        {
            OnSuccess();
        }
    }

    void Update()
    {
        float rotationAngle;
        //This uses InControl to check whether our last input was gamepad or mouse
        /*
        if (playerActions.LastInputType == BindingSourceType.DeviceBindingSource)
        {
            //If it's a gamepad, get the stick input and translate it to a useful rotation angle, then rotate the lockpick
            Vector2 stickInput = InputManager.ActiveDevice.LeftStick.Value;

            rotationAngle = (Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg) - 90f;
            Lockpick2.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
        }
        */
        //else
        {
            //For mouse, figure out how much the mouse moved and use that to calculate a rotational angle
            float MousePosDiff = LastMousePos.x - Input.mousePosition.x;

            rotationAngle = (MousePosDiff * 300) / 360.0f;
            Lockpick2.Rotate(0, 0, rotationAngle);
        }

        
        //Get the angle
        float lockpickAngle = Lockpick2.rotation.eulerAngles.z;

        //Unlike Thief3, the lockpick doesn't precisely match the angle of rotation - or maybe it does, but it's 3D and really hard to tell.
        //Lockpick 1 also moves "randomly" as well but doesn't seem to affect the actual success, just cosmetic

        //Display it for the player as a hint
        CurrentAngleText.text = lockpickAngle.ToString("F0");

        //Figure out how close we are, on a sliding scale, to the target angle
        float closeness = Mathf.Abs(targetAngle - lockpickAngle) % 360;
        closeness = (closeness > 180f ? 360 - closeness : closeness);
        closeness = 1.0f - (closeness/360f);

        //Set the alpha of the tumbler relative to how close we are to the target angle
        float alpha = Mathf.Pow(closeness, 7) * 0.8f;

        Color c = Tumblers[CurrentTumbler].InnerCircle.color;
        c.a = alpha;
        Tumblers[CurrentTumbler].InnerCircle.color = c;

        //Figure out if we're in the success range - it's too hard to ask the player to hit a single angle precisely, so we give them a small range that's valid
        InSuccessRange = (lockpickAngle > targetAngle - TargetAngleSuccessRange && lockpickAngle < targetAngle + TargetAngleSuccessRange);

        //If we're not there and they try to move anyway, blink the current tumbler red so they know they failed
        if (!InSuccessRange)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Blink red
                Tumblers[CurrentTumbler].InnerCircle.DOColor(Color.red, 0.1f).SetLoops(6, LoopType.Yoyo);
            }
        }
        //If we are in the success range, then move to the next tumbler and update the tumblers
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Lock tumbler
                CurrentTumbler += 1;
                UpdateTumblers();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //We use this to calculate the difference in mouse position from frame to frame
        LastMousePos = Input.mousePosition;
    }

    public override string GetFriendlyName()
    {
        return "Thief 4";
    }
}
