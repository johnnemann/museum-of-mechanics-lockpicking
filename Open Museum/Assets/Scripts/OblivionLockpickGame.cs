using DG.Tweening;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//In Oblivion, the player gets a side view of a lock, with some number of tumblers. They proceed through, trying to pick each tumbler in turn by hitting "pick" to knock it
//upwards and then timing the next press of "pick" so that they get it at the highest point
public class OblivionLockpickGame : LockpickGame
{

    //How many tumblers are successfully locked
    bool[] TumblersLocked = new bool[5];

    //Index of the current lockpick position
    int CurrentLockpickPosition;

    //In order to stop the player from overshooting, we introduce some delay into movement using a timer
    //This also lets us finish animating the movement before they can move again
    float MovementDelayCountdown;
    public float MovementDelay = 0.3f;

    //This tracks whether we have knocked a tumbler or not
    bool Picking = false;

    //When this is true the tumbler is ready to be locked in place
    bool InSuccessWindow = false;

    //The player has a limited window of time around the success point to hit the lock button
    public float SuccessWindowTime = 0.1f;

    float SuccessTimer;

    
    bool AtTop = false;

    Player ThePlayer;


    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;

    //The visual representation of the lockpick and tumblers
    public GameObject LockpickObject;
    public GameObject[] Tumblers;
    RectTransform LockpickTransform;

    //The valid positions for the lockpick at each step
    public Vector2[] LockpickPositions;

    //The tumbler positions at the top and bottom of their movement
    public Vector2[] TumblerTopPositions;
    public Vector2[] TumblerBottomPositions;



    public override void BeginLockpicking(Player player)
    {
        ThePlayer = player;
        ThePlayer.FreezePlayer();

        GamePanel.SetActive(true);

        //Put the lockpick at the start position
        LockpickTransform = LockpickObject.GetComponent<RectTransform>();
        LockpickTransform.anchoredPosition = LockpickPositions[0];
        CurrentLockpickPosition = 0;
        MovementDelayCountdown = MovementDelay;

        //TODO: Expose a difficulty setting that sets this up differently
        ResetTumblerLocking();

        SetTumblersLocked();
    }

    //Right now this is hard-coded to set the first three tumblers as locked, and only the last two as moving
    void ResetTumblerLocking()
    {
        
        TumblersLocked[0] = true;
        TumblersLocked[1] = true;
        TumblersLocked[2] = true;
        TumblersLocked[3] = false;
        TumblersLocked[4] = false;
    }

    //this sets the visual representation of the tumblers in the correct positions
    void SetTumblersLocked()
    {
        for (int i = 0; i < 5; i++)
        {
            if (TumblersLocked[i])
            {
                Tumblers[i].GetComponent<RectTransform>().anchoredPosition = TumblerTopPositions[i];
            }
            else
            {
                Tumblers[i].GetComponent<RectTransform>().anchoredPosition = TumblerBottomPositions[i];
            }
        }
    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    //Check bounds and move the lockpick to the right place
    public void MoveLockpickLeft()
    {
        if (CurrentLockpickPosition < 4)
        {
            CurrentLockpickPosition += 1;
            //Use DOTween to move smoothly there over time
            LockpickTransform.DOAnchorPos(LockpickPositions[CurrentLockpickPosition], 0.2f);
        }
    }

    public void MoveLockpickRight()
    {
        if (CurrentLockpickPosition > 0)
        {
            CurrentLockpickPosition -= 1;
            //Use DOTween to move smoothly there over time
            LockpickTransform.DOAnchorPos(LockpickPositions[CurrentLockpickPosition], 0.2f);
        }
    }

    //This is a callback for tracking the animation state of the tumbler
    public void FinishPick()
    {
        Picking = false;
    }

    //This is called by the tween on each step of the loop (at the top and bottom). So, to know whether it's the top or bottom we have a flag
    //If the flag is true, we don't do anything because we're at the bottom now. If not, we set it and mark that we're in the success window
    //and set the timer
    public void SetSuccessWindow()
    {
        if (AtTop)
        {
            AtTop = false;
        }
        else
        {
            AtTop = true;
            InSuccessWindow = true;
            SuccessTimer = SuccessWindowTime;
        }
    }

    //Check to see if we're done - if all tumblers are locked, then the player has won the minigame
    void CheckSuccess()
    {
        bool Success = true;
        for (int i = 0; i < 5; i++)
        {
            if (!TumblersLocked[i])
            {
                Success = false;
            }
        }

        if (Success)
        {
            OnSuccess();
        }
    }

    //This animates the lockpick and the tumbler
    public void Pick()
    {
        Picking = true;
        //Use DOTween to rotate the lockpick at its pivot point and bounce it. When it's done, set the pick flag to false in the callback
        LockpickTransform.DORotate(Quaternion.AngleAxis(2, Vector3.forward).eulerAngles, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(FinishPick);
        if (!TumblersLocked[CurrentLockpickPosition])
        {
            //Move the tumbler. Send the tumbler up and back down, and when it reaches the top put us in the success window using the SetSuccessWindow callback
            Tumblers[CurrentLockpickPosition].GetComponent<RectTransform>().DOAnchorPos(TumblerTopPositions[CurrentLockpickPosition], 0.3f).SetLoops(2, LoopType.Yoyo).OnStepComplete(SetSuccessWindow).SetId("Tumbler");
        }
    }

    //Set the locked status for the current tumbler
    public void LockTumbler()
    {
        TumblersLocked[CurrentLockpickPosition] = true;
        CheckSuccess();
    }

    //Stop the animations, reset the tumblers to starting state
    public override void OnFailure()
    {
        DOTween.Kill("Tumbler");
        ResetTumblerLocking();
        SetTumblersLocked();
    }

    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    void Update()
    {
        //First, update our movement delay
        if (MovementDelayCountdown > 0)
        {
            MovementDelayCountdown -= Time.deltaTime;
        }
        //Then, update our success window timer
        if (SuccessTimer > 0)
        {
            SuccessTimer -= Time.deltaTime;
        }
        //When the player hits "pick", if we're at the point where the tumbler is going up, test to see if we're in the success window
        if (Input.GetKeyDown(KeyCode.Space) && Picking == true)
        {
            //If we are, then kill the animation (so it doesn't go back down), then lock the tumbler
            if (SuccessTimer > 0)
            {
                DOTween.Kill("Tumbler");
                AtTop = false;
                LockTumbler();
            }
            //Oops, the player failed
            else
            {
                OnFailure();
            }
        }
        //If pick is pressed but we're not already bouncing a tumbler, then do that
        else if (Input.GetKeyDown(KeyCode.Space) && Picking == false && MovementDelayCountdown <= 0)
        {
            Pick();
            MovementDelayCountdown = MovementDelay;
        }

        //Move the lockpick left or right
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) && MovementDelayCountdown <= 0)
        {
            MoveLockpickLeft();
            MovementDelayCountdown = MovementDelay;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) && MovementDelayCountdown <= 0)
        {
            MoveLockpickRight();
            MovementDelayCountdown = MovementDelay;
        }

        if(Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
    }

    public override string GetFriendlyName()
    {
        return "Oblivion";
    }
}
