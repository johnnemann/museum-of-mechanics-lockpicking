using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//We track the state we're in to correctly handle input - moving is when the player is moving the pick, picking is the picking animation,
//and success is used when we've succeeded so we can consume additional input
public enum LeCluePickingState
{
    MOVING,
    PICKING,
    SUCCESS,
}


//In Jenny LeClue, the player moves a 2D hairpin and paperclip combo to the right position in order to open the lock. This happens three times before ultimate success.
public class JennyLeClueLockpickGame : LockpickGame
{
    //Player object
    Player thePlayer;

    //Here we keep track of our colliders - the hairpin is what the player is moving, and the targets are what the player needs to hit
    public BoxCollider2D hairpinCollider;
    public BoxCollider2D[] targetColliders;

    //The index of our current target collider in the list of colliders
    int currentTarget = 0;

    LeCluePickingState currentState;

    //Rotation and movement speed for the player controls, tunable in the editor
    public float RotateSpeed = 5.0f;
    public float MoveSpeed = 5.0f;

    //How long the player needs to hold the pick button in order to succeed
    public float pickHoldTime = 1.0f;

    //For keeping track of the time it's held down
    float pickTimer = 0.0f;

    //UI
    //The panel object containing the game, activated when we start lockpicking
    public GameObject Panel;

    //The hairpin, paperclip, and lock objects. Since we're primarily concerned with moving them around, we want the RectTransform components
    public RectTransform hairpin;
    public RectTransform paperclip;
    public RectTransform Lock;

    //The starting position of the hairpin so we can reset it
    public Vector3 initialHairpinPosition;

    //The image for the timer shown when the player is holding down Pick
    public Image holdTimerImage;
    //The gameobject of the whole timer
    public GameObject holdTimer;


    //Called when the player interacts with the lock to start the whole minigame
    public override void BeginLockpicking(Player player)
    {
        //Set the game's panel active, grab the player object, and freeze the player in place
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        //Initialize all the lock info
        SetupLock();
    }

    //The function puts the lock in a good starting state, and is called to start or reset the game
    void SetupLock()
    {
        //Reset the rotations of the animated objects and set the player state back to Moving
        Lock.localRotation = Quaternion.Euler(Vector3.zero);
        hairpin.localRotation = Quaternion.Euler(Vector3.zero);
        currentState = LeCluePickingState.MOVING;

        //Start with our first target
        //TODO: randomize?
        currentTarget = 0;

        //Make sure the hold timer is hidden
        holdTimer.SetActive(false);
    }

    //Reverse everything we did in BeginLockpicking and end the game
    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //Not used - this game can't really be failed
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //When the player succeeds we open the in-world lock and end the minigame
    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    //used to toggle the display of the active targets when the player hits 'T' for the hint
    void ShowHideTargets()
    {
        //Go through all of our colliders and set the text component (for the numbering) and the image enabled or disabled
        foreach (BoxCollider2D collider in targetColliders)
        {
            Image im = collider.GetComponent<Image>();
            TextMeshProUGUI text = collider.GetComponentInChildren<TextMeshProUGUI>();
            im.enabled = !im.enabled;
            text.enabled = !text.enabled;
        }
        //Do the same for the collider image on the hairpin
        hairpinCollider.GetComponent<Image>().enabled = !hairpinCollider.GetComponent<Image>().enabled;
    }

    //Delay success to give the player a second to appreciate it and to clear the input buffer
    IEnumerator WaitForSuccess()
    {
        yield return new WaitForSeconds(0.3f);
        OnSuccess();
    }

    //Here we handle player input
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            ShowHideTargets();
        }
        //If the player is in the MOVING state, then we want to consider the movement actions and respond to them appropriately
        if (currentState == LeCluePickingState.MOVING)
        {
            //We accumulate rotation and movement depending on what's pressed this frame, and multiply by Time.deltaTime to make it framerate-independent
            float rotationAmount = 0.0f;
            float moveAmount = 0.0f;
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                rotationAmount = Time.deltaTime * RotateSpeed;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                moveAmount = Time.deltaTime * MoveSpeed;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                rotationAmount = Time.deltaTime * -RotateSpeed;
                
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                moveAmount = Time.deltaTime * -MoveSpeed;
                
            }
            //Apply the rotation and the movement
            hairpin.Rotate(new Vector3(0, 0, rotationAmount));
            hairpin.localPosition += new Vector3(moveAmount, 0);

            //limit position/rotation to inside bounds - these are hardcoded magic numbers based on what looked right
            if (hairpin.localEulerAngles.z > 35 && hairpin.localEulerAngles.z < 180)
            {
                hairpin.localRotation = Quaternion.Euler(new Vector3(0, 0, 35));
            }
            if (hairpin.localEulerAngles.z < 325 && hairpin.localEulerAngles.z > 180)
            {
                hairpin.localRotation = Quaternion.Euler(new Vector3(0, 0, -35));
            }
            if (hairpin.localPosition.x > 50)
            {
                hairpin.localPosition = new Vector3(50, 0, 0);
            }
            if (hairpin.localPosition.x < -50)
            {
                hairpin.localPosition = new Vector3(-50, 0, 0);
            }

            //If the hairpin collider is touching the current target collider, then indicate to the player with a little animation (and sound, eventually!)
            if (hairpinCollider.IsTouching(targetColliders[currentTarget]))
            {
                //Move lock slightly 
                Lock.Rotate(new Vector3(0, 0, -3));
                //pop up "hold" prompt
                holdTimer.SetActive(true);
                //put us in picking state
                currentState = LeCluePickingState.PICKING;
                pickTimer = 0f;
                holdTimerImage.fillAmount = 0;
            }
        }
        //If we're in the picking state, then we're just interested in whether the player is holding the button and for how long
        else if (currentState == LeCluePickingState.PICKING)
        {
            //listen for button hold
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //Add to the timer for each moment the player holds the button
                pickTimer += Time.deltaTime;
                //We use Unity's filled sprite features to give a progress-bar-like timer here
                holdTimerImage.fillAmount = (pickTimer / pickHoldTime);
                if (pickTimer >= pickHoldTime)
                {
                    //animate lock
                    Lock.Rotate(new Vector3(0, 0, -8));
                    //Hide "Hold" prompt
                    holdTimer.SetActive(false);
                    //move to next stage
                    currentState = LeCluePickingState.MOVING;
                    //If we haven't hit the last target, then increment the target by one. Otherwise, we've succeeded!
                    if (currentTarget < targetColliders.Length - 1)
                    {
                        currentTarget += 1;
                    }
                    else
                    {
                        currentState = LeCluePickingState.SUCCESS;
                        StartCoroutine(WaitForSuccess());
                    }
                    
                }
            }
        }
    }

    public override string GetFriendlyName()
    {
        return "Jenny LeClue: Detectivu";
    }
}
