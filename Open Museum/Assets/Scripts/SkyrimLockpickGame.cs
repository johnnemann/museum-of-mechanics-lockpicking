using TMPro;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In Skyrim, the lockpicking is a further evolution of the Thief 3 game: the player moves the lockpick and tries to match the angle. However,
//they have to try to rotate the lock when they're in the right zone, instead of holding it there (Thief 3) or just pressing a button (Thief 4, Fallout 3)
//The lock rotation gives analog feedback about how close they are, rotating more or less depending. If the player is close enough, the lock rotates all the way
//and opens
public class SkyrimLockpickGame : LockpickGame
{
    //The lockpick that actually moves. The other one is Lockpick 1, but doesn't move at all so it isn't needed here
    public RectTransform Lockpick2;

    //The lock plate that rotates when the player tests their angle
    public RectTransform LockPlate;

    //We keep track of our mouse position so that we can change the angle as the player moves the mouse
    Vector3 StartMousePos;
    Vector3 LastMousePos;

    //A reference to our player object so that we can lock it in place while the game is going on
    Player ThePlayer;

    //This is the success area the player is trying to find
    float targetAngle;

    //How long the lock takes to rotate fully
    public float LockRotationDuration = 1.0f;
    float LockRotationTime = 0.0f;

    //In order to stop the lock moving immediately upon the player entering the game, we put in a short delay before we accept input
    public float ActivationDelay = 1.0f;

    float ActivationTimer = 0.0f;

    public float FailureDelay = 0.5f;
    float FailureTimer;
    bool Failing = false;


    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;

    //We tell the player their current angle and the target angle in the museum to give a more accurate view into the game's workings
    //TODO: hide this behind a hint prompt
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
        //In Thief 3, the angle can be anywhere on the circle, but here it's limited to half a circle
        targetAngle = Random.Range(0, 180);

        //Display the target angle for the player to aim for
        //TODO: Hide this behind a hint prompt
        TargetAngleText.text = targetAngle.ToString("F0");
    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    //On failure, just reset the lock. In the game, the lockpick breaks, but since we don't have an inventory or an economy, that's immaterial
    public override void OnFailure()
    {
        //TODO: Break lockpick
        //Reset to beginning

        LockRotationTime = 0.0f;
        LockPlate.rotation = Quaternion.identity;
        Lockpick2.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        ActivationTimer = ActivationDelay;
        Failing = false;
    }

    //On success, open the in-world lock and end the minigame
    public override void OnSuccess()
    {
        EndLockpicking();
        OpenLock();
    }

    void Update()
    {
        //If we're past the initial interaction delay, then we can test input
        if (ActivationTimer <= 0)
        {

            float rotationAngle;

            //This code depends on InControl to figure out whether or not the player is using mouse or controller,
            //so I have commented it out, but left it here so that you can see the math involved in the controller angle
            //calculation
            //Figure out if the player is using a gamepad or a mouse
            /*
            if (playerActions.LastInputType == BindingSourceType.DeviceBindingSource)
            {
                //For gamepad, get the stick angle and convert it to a rotation angle in our range
                Vector2 stickInput = InputManager.ActiveDevice.LeftStick.Value;
                rotationAngle = (Mathf.Atan2(stickInput.y, stickInput.x) * Mathf.Rad2Deg) - 90f;
                rotationAngle = Mathf.Clamp(rotationAngle, -90, 90);
                Lockpick2.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
            }
            */
            //else
            {
                //For mouse, figure out the amount of movement the last frame and use that to figure out how much our angle changes
                float MousePosDiff = LastMousePos.x - Input.mousePosition.x;

                rotationAngle = (MousePosDiff * 100) / 360.0f;
                rotationAngle += Lockpick2.rotation.eulerAngles.z;

                //Make sure it's between our valid angles
                if (rotationAngle < 270 && rotationAngle > 180)
                {
                    rotationAngle = 270;
                }
                else if (rotationAngle > 90 && rotationAngle < 180)
                {
                    rotationAngle = 90;
                }
                Lockpick2.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);

            }


            //Get the angle that we use to test for success - we have to convert it to within the range 0-180 that we're using
            float lockpickAngle = (Lockpick2.rotation.eulerAngles.z <= 90 ? (-Lockpick2.rotation.eulerAngles.z) + 90f : (-Lockpick2.rotation.eulerAngles.z) + 450f);

            //Unlike Thief3, the lockpick doesn't precisely match the angle of rotation - or maybe it does, but it's 3D and really hard to tell.
            //Lockpick 1 also moves "randomly" as well but doesn't seem to affect the actual success, just cosmetic

            CurrentAngleText.text = lockpickAngle.ToString("F0");

            //If the player is pressing the button to rotate the lock, then handle that logic
            if (Input.GetMouseButton(0))
            {
                //Add to the total time we've been rotating
                LockRotationTime += Time.deltaTime;
                //Rotate the lock by an amount proportional to how close we are to the correct angle
                float closeness = Mathf.Abs(targetAngle - lockpickAngle) % 180;
                closeness = 1.0f - (closeness / 180f);
                Quaternion targetRotation = Quaternion.AngleAxis(closeness * 90f, Vector3.forward);
                //TODO: Tween
                LockPlate.rotation = Quaternion.Lerp(Quaternion.identity, targetRotation, LockRotationTime / LockRotationDuration);

                //If we've reached the duration of rotating, figure out if the player succeeded
                if (LockRotationTime >= LockRotationDuration)
                {
                    //See if we're close and if so, unlock. Otherwise, we might fail
                    //TODO: use a tunable percentage here
                    if (closeness > 0.95f)
                    {
                        OnSuccess();
                    }
                    else
                    {
                        //If we're not close, put us into the failure state - we haveb't failed YET, but if we keep trying to force the lock we will
                        if (!Failing)
                        {
                            FailureTimer = FailureDelay;
                            Failing = true;
                        }
                        else
                        {
                            FailureTimer -= Time.deltaTime;
                            if (FailureTimer <= 0f)
                            {
                                OnFailure();
                            }
                        }
                    }
                }
            }
            //Otherwise rotate the lock back to the starting position
            else
            {
                LockRotationTime = 0.0f;
                LockPlate.rotation = Quaternion.identity;
                Failing = false;
            }
        }
        //Count down to activating input for the player after starting
        else
        {
            ActivationTimer -= Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        LastMousePos = Input.mousePosition;
    }

    public override string GetFriendlyName()
    {
        return "Skyrim";
    }
}
