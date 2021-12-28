using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In this game, the player is attempting to raise each tumbler up to a particular position, before locking it and moving to the next. The challenge comes in the form of a timer,
//so the player only has a short time to do this
public class AlphaProtocolLockpickGame : LockpickGame
{
    Player thePlayer;

    //Index of the tumbler the player is manipulating
    int currentTumbler = 0;

    //The correct solution - determined randomly at the start of the game
    float[] tumblerTargetValues;

    //Tracking the movement of the current one
    float currentTumblerValue;

    //Essentially the speed at which the tumbler moves
    public float MovementScaleValue = 10f;

    //How close to the actual value the player needs to be for it to count as a success
    public float SuccessRange = 0.05f;

    //Don't read input for a little while so that the starting click doesn't get read as an attempt to lock the tumbler
    public float BeginDelay = 0.2f;

    float inputDelay = 0f;

    //Tunable value for the timer
    public float TimerStartValue = 20.0f;

    float currentTimerValue;

    //UI
    public GameObject Panel;

    //A list of all the tumbler objects. See AlphaProtocolTumbler script for more information
    public List<AlphaProtocolTumbler> tumblers;

    //The decorations on the side of the UI that indicate how far along we are - not necessary for the game but fun to make
    public List<GameObject> progressIndicatorImages;

    //An indicator that tells the player how close they are to the target area
    public GameObject closenessIndicator;


    //We use two images to simulate the timer bar closing in from either side - again, just a fun little UI matching decoration
    public Image timerBarLeft;
    public Image timerBarRight;

    //Text objects to show the timer value and whether or not the player has failed
    public TMPro.TextMeshProUGUI timerText;

    public TMPro.TextMeshProUGUI failureText;

    //As it approaches failure, the timer text changes
    public Color failureColor;
    public Color regularColor;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupTumblers();
        inputDelay = BeginDelay;

    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If the player fails, just end the game
    public override void OnFailure()
    {
        Debug.Log("Failed!");
        EndLockpicking();
    }

    //On success, open the in-world lock and end the game
    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
        EndLockpicking();
    }

    //Sets up the initial state of the lock
    public void SetupTumblers()
    {
        //Initialize the timer, the target values, current tumbler and its value
        currentTimerValue = TimerStartValue;
        tumblerTargetValues = new float[tumblers.Count];
        currentTumbler = 0;
        currentTumblerValue = 0;
        //generate values for tumblers between 0.4 and 0.85, which seem to be reasonable goal values (too high or too low looks silly)
        int i = 0;
        foreach (AlphaProtocolTumbler t in tumblers)
        {
            //Generate a range and set that in the tumbler image
            float percentage = Random.Range(0.4f, 0.85f);
            t.SetBottomTumblerSize(percentage);
            //Set colors back to original values
            t.ResetColors();
            //Clear all selectors (the little arrows at the bottom)
            t.selectorImage.SetActive(false);
            tumblerTargetValues[i] = 1f - percentage;
            i++;
        }

        //This sets the little arrow at the bottom to be visible for the current tumbler
        tumblers[currentTumbler].selectorImage.SetActive(true);
        //Clear progress indicators
        foreach (GameObject image in progressIndicatorImages)
        {
            image.SetActive(false);
        }

        closenessIndicator.SetActive(false);
        timerText.color = regularColor;
    }

    void Update()
    {
        //The player can always exit, so put this before the input delay code
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        //When the game starts, we don't want the player's leftover click from activating to go into the game, so we have a short delay to clear the input buffer
        //We also use this when the player has failed
        if (inputDelay > 0)
        {
            inputDelay -= Time.deltaTime;
            return;
        }
        //This is for Unity input - only let gravity apply to the tumbler if we haven't moved or locked them
        bool TumblerMovedOrLocked = false;
        //Take input and move tumblers
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            Debug.Log("UP");
            //Move the tumbler according to the speed, scaled by time to be framerate independent
            float newTumblerValue = currentTumblerValue + (MovementScaleValue * Time.deltaTime);

            //Check a reasonable bound and don't move past that
            if (newTumblerValue <= 100)
            {
                currentTumblerValue = newTumblerValue;
            }
            MoveTumbler();
            TumblerMovedOrLocked = true;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            Debug.Log("DOWN");
            float newTumblerValue = currentTumblerValue - (MovementScaleValue * Time.deltaTime);

            if (newTumblerValue >= 0)
            {
                currentTumblerValue = newTumblerValue;
            }
            MoveTumbler();
            TumblerMovedOrLocked = true;
        }
        //If the player thinks this tumbler is in position, they press a button to lock it in place
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LockTumbler();
            TumblerMovedOrLocked = true;
        }

        if (!TumblerMovedOrLocked)
        {
            //Also, apply downward pressure on the tumblers - they fall on their own if not manipulated by the player
            float gravityTumblerValue = currentTumblerValue - ((MovementScaleValue) * Time.deltaTime);

            if (gravityTumblerValue >= 0)
            {
                currentTumblerValue = gravityTumblerValue;
            }
            MoveTumbler();
        }
        //Update closeness indicator - if we're within the success range on either side of the target value then show the indicator, otherwise hide it
        if (currentTumblerValue > (tumblerTargetValues[currentTumbler] * 100) - SuccessRange && currentTumblerValue < (tumblerTargetValues[currentTumbler] * 100) + SuccessRange)
        {
            closenessIndicator.SetActive(true);
        }
        else
        {
            closenessIndicator.SetActive(false);
        }

        //And update the timer text/bar
        currentTimerValue -= Time.deltaTime;
        if (currentTimerValue <= 0)
        {
            //show a failure indicator
            StartCoroutine(FailureCoroutine());
        }
        else
        {
            //Set the current timer text
            timerText.text = currentTimerValue.ToString("F");
            //To make it look like a single bar shrinking towards the middle, we reduce each bar, but one shrinks from the left, one from the right
            timerBarLeft.fillAmount = currentTimerValue / TimerStartValue;
            timerBarRight.fillAmount = currentTimerValue / TimerStartValue;
            if (currentTimerValue / TimerStartValue <= 0.2f)
            {
                //If we're getting close to time out, start showing the timer in red
                timerText.color = failureColor;
            }
            else
            {
                timerText.color = regularColor;
            }
        }
    }

    IEnumerator FailureCoroutine()
    {
        //Display failure text - blink it?
        failureText.gameObject.SetActive(true);
        inputDelay = 5.0f;
        yield return new WaitForSeconds(3.0f);
        failureText.gameObject.SetActive(false);
        OnFailure();
    }

    void MoveTumbler()
    {
        //Slide the current tumbler in the appropriate direction based on the input
        //This only works because the tumblers start at position 0; otherwise we would need to work relative to their starting Y position
        tumblers[currentTumbler].transform.localPosition = new Vector3(tumblers[currentTumbler].transform.localPosition.x, currentTumblerValue, tumblers[currentTumbler].transform.localPosition.z);
    }

    //Here we check the tumbler movement against the target values
    void LockTumbler()
    {
        //Upping the success range here because the auto falling leads to it misleading the player. This way it misleads the designer instead, haha, whoops
        //The right solution is probably making two values, an "alert range" and a "success range", or making the alert indicator alpha up when closer instead of just on/off
        if (currentTumblerValue > ((tumblerTargetValues[currentTumbler]*100) - (SuccessRange * 1.5f)) && currentTumblerValue < ((tumblerTargetValues[currentTumbler] * 100) + (SuccessRange*1.5f)))
        {
            //Success!
            //lock tumbler, proceed to next, and activate progress indicator image
            tumblers[currentTumbler].selectorImage.SetActive(false);
            tumblers[currentTumbler].SetSuccessColor();
            progressIndicatorImages[currentTumbler].SetActive(true);
            if (currentTumbler < tumblers.Count - 1)
            {
                currentTumbler += 1;
                tumblers[currentTumbler].selectorImage.SetActive(true);
                currentTumblerValue = 0;
            }
            else
            {
                //If we're through all the tumblers, we've won
                OnSuccess();
            }
        }

    }

    public override string GetFriendlyName()
    {
        return "Alpha Protocol";
    }
}
