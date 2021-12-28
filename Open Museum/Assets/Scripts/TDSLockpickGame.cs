using TMPro;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//In Thief 3, the player moves the lockpick to a particular angle, and if it's close enough the ring starts moving. If they hold there for a moment, they succed and move to the
//next ring
public class TDSLockpickGame : LockpickGame
{
    //The rings as transforms (for easy manipulation)
    public RectTransform Ring1;
    public RectTransform Ring2;
    public RectTransform Ring3;
    public RectTransform Ring4;

    //The lockpick that actually moves. The other one is Lockpick 1, but doesn't move at all so it isn't needed here
    public RectTransform Lockpick2;

    //We keep track of our mouse position so that we can change the angle as the player moves the mouse
    Vector3 StartMousePos;
    Vector3 LastMousePos;

    //A reference to our player object so that we can lock it in place while the game is going on
    Player ThePlayer;

    //This is generated with every ring - this is the success area the player is trying to find
    float targetAngle;

    //The success range gives the player a small range of degrees around the target angle which are considered "correct". 
    //without this the player would have to be right on, which is a lot to ask with imprecise controls
    //The TargetAngleJiggleRange moves the rings when the lockpick is inside this larger area, giving the player feedback that they are getting close 
    //The picking is considered successful if the player holds the pick at targetAngle +- TargetAngleSuccessRange for SuccessHoldTime seconds
    public float TargetAngleSuccessRange = 5.0f;
    public float TargetAngleJiggleRange = 15.0f;
    public float SuccessHoldTime = 1.5f;

    //When the player enters the range we set this to true, and as long as they don't leave it for SuccessHoldTime they will succeed
    bool InSuccessRange = false;

    //Keeps track of the current time remaining on the hold
    float SuccessTimer;

    //A reference to our current ring, for animation purposes
    RectTransform currentRing;


    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;
    //This is for debug feedback when the player is close to the target or on the target
    public TextMeshProUGUI StatusText;
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

        //Set up the initial values - we're on the first ring, with a random target, we're not succeeding, etc
        currentRing = Ring1;
        targetAngle = Random.Range(0, 360);

        InSuccessRange = false;

        //TODO: hide this behind a hint prompt
        TargetAngleText.text = targetAngle.ToString("F0");

    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    //There's really no failure on this game - you just don't proceed
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
    }

    void Update()
    {

        //Here we figure out the player's input and translate that to an angle for the lockpick
        float rotationAngle;
        //This code depends on InControl to see what control scheme the player is using, so will not compile. I left it in, commented out
        //in order to show the math that goes into to finding the controller stick angle
        /*
        if (playerActions.LastInputType == BindingSourceType.DeviceBindingSource)
        {
            Vector2 stickInput = InputManager.ActiveDevice.LeftStick.Value;
            rotationAngle = (Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg) - 90f;
            Lockpick2.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
        }
        */
        //else
        {
            float MousePosDiff = LastMousePos.x - Input.mousePosition.x;

            rotationAngle = (MousePosDiff * 300) / 360.0f;
            Lockpick2.Rotate(0, 0, rotationAngle);
        }

        Lockpick2.Rotate(0, 0, rotationAngle);

        float lockpickAngle = Lockpick2.rotation.eulerAngles.z;

        //Display the current angle as a hint
        //TODO: hide this behind a hint toggle
        CurrentAngleText.text = lockpickAngle.ToString("F0");
        StatusText.text = "";

        //rotateBig and rotateSmall are whether the lock moves a little (the player is close but not there) or a lot (the player is in success range and should hold)
        bool rotateBig = false, rotateSmall = false;

        if (!InSuccessRange)
        {
            //Check to see if we're in the success range now
            if (lockpickAngle > targetAngle - TargetAngleSuccessRange && lockpickAngle < targetAngle + TargetAngleSuccessRange)
            {
                //Start countdown
                InSuccessRange = true;
                SuccessTimer = SuccessHoldTime;
                //Do strong jiggle
                StatusText.text = "On target!";
                rotateBig = true;
            }
            else if (lockpickAngle > targetAngle - TargetAngleJiggleRange && lockpickAngle < targetAngle + TargetAngleJiggleRange)
            {
                //start jiggling
                StatusText.text = "Close!";
                rotateSmall = true;
            }
        }
        //If inSuccessRange is true, we're either still in it or we've left
        else
        {
            //If we're still in it...
            if (lockpickAngle > targetAngle - TargetAngleSuccessRange && lockpickAngle < targetAngle + TargetAngleSuccessRange)
            {
                //continue countdown
                SuccessTimer -= Time.deltaTime;

                if (SuccessTimer <= 0)
                {
                    //One ring worked!
                    //On to the next, unless it's the last ring, in which case we're done.
                    currentRing.rotation = Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));
                    if (currentRing == Ring4)
                    {
                        //We're done!
                        OnSuccess();
                        EndLockpicking();
                    }
                    else if (currentRing == Ring3)
                    {
                        currentRing = Ring4;

                    }
                    else if (currentRing == Ring2)
                    {
                        currentRing = Ring3;
                    }
                    else if (currentRing == Ring1)
                    {
                        currentRing = Ring2;
                    }
                    targetAngle = Random.Range(0, 360);
                    TargetAngleText.text = targetAngle.ToString("F0");
                    InSuccessRange = false;
                }
                else
                {
                    rotateBig = true;
                    StatusText.text = "On target!";
                }
            }
            //Oops, we left it! stop everything
            else
            {
                rotateSmall = false;
                rotateBig = false;
                //Stop countdown
                InSuccessRange = false;
            }
        }

        LastMousePos = Input.mousePosition;


        //Animations of the rings for success indication
        if (rotateBig)
        {
            currentRing.Rotate(0, 0, Random.Range(-5.0f, 5.0f));
        }
        else if (rotateSmall)
        {
            currentRing.Rotate(0, 0, Random.Range(-2.5f, 2.5f));
        }
        else
        {
            currentRing.rotation = Quaternion.AngleAxis(0, new Vector3(0, 0, 1));
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
    }

    public override string GetFriendlyName()
    {
        return "Thief 3 (Deadly Shadows)";
    }
}
