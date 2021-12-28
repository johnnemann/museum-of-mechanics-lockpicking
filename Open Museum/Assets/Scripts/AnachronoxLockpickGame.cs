using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In Anachronox, the lockpicking game is a number-guessing game. The player chooses a number for each tumbler, and the game indicates how close it is to the target number
//There's a time limit to make it challenging
public class AnachronoxLockpickGame : LockpickGame
{

    Player ThePlayer;

    //The numbers the player is trying to guess
    int[] CurrentTargetNumbers = new int[5];

    // the tumbler the player is currently guessing
    int CurrentTumbler;

    //The displayed values of the tumblers
    int[] CurrentTumblerDigits = new int[5];

    //This is how we control how many of the tumblers the game will ask the player to solve. In the real game, the difficulty varies depending on which lock. Here,
    //it's configurable in the editor
    //TODO: eventually have this be configurable at runtime
    public bool[] ActiveDigits = new bool[5];

    //How much time the player has to solve the puzzle
    float TimeRemaining;

    public float TimeLimit;

    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;

    //The text objects of the tumbler display
    public TextMeshProUGUI[] CurrentTumblerDigitText;

    //The tumbler objects
    public GameObject[] Tumblers;

    //Timer text
    public TextMeshProUGUI TimeRemainingText;

    //A meter that shows the player how close their guess is - the higher the meter is, the closer
    public Image Meter;

    //The visual representation of the player's lockpick
    public RectTransform LockpickObject;

    //Where the lockpick starts
    public Vector2 LockpickStartPos;


    //Called when the player starts the lockpicking game. It sets up our initial values
    public override void BeginLockpicking(Player player)
    {
        GamePanel.SetActive(true);

        player.FreezePlayer();

        ThePlayer = player;

        TimeRemaining = TimeLimit;
        TimeRemainingText.text = TimeRemaining.ToString("F1");

        //Generate random target values between 0 and 9 for each tumbler
        for (int i = 0; i < 5; i++)
        {
            CurrentTargetNumbers[i] = Random.Range(0, 10);
            CurrentTumblerDigits[i] = 0;
        }

        SetTumblerDigits();

        CurrentTumbler = 0;

        Meter.fillAmount = 0;

        LockpickObject.anchoredPosition = LockpickStartPos;

        foreach(GameObject tumbler in Tumblers)
        {
            tumbler.SetActive(true);
        }


    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    //If the player fails, just end the game
    //TODO: add delay?
    public override void OnFailure()
    {
        EndLockpicking();
    }

    //If they succeed, open the in-world lock and then end the game
    //TODO: ADd delay?
    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
        EndLockpicking();
    }

    void Update()
    {
        //First, update the timer and the visual display
        TimeRemaining -= Time.deltaTime;

        TimeRemainingText.text = TimeRemaining.ToString("F1");

        //If we've run out, the player fails
        if (TimeRemaining <= 0)
        {
            OnFailure();
        }

        //If the player guesses, we check their input value against the target
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GuessCurrentTumbler();
        }

        //Raise the guessed value by one
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            IncrementGuess(CurrentTumbler);
        }
        //Lower the guessed value by one
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            DecrementGuess(CurrentTumbler);
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
    }

    //Update the number, if we go above 9 wrap around to 0
    public void IncrementGuess(int tumbler)
    {
        CurrentTumblerDigits[tumbler] += 1;
        if (CurrentTumblerDigits[tumbler] > 9)
        {
            CurrentTumblerDigits[tumbler] = 0;
        }
        SetTumblerDigits();
    }

    //Update the number, if we go below 0 wrap around to 9
    public void DecrementGuess(int tumbler)
    {
        CurrentTumblerDigits[tumbler] -= 1;
        if (CurrentTumblerDigits[tumbler] < 0)
        {
            CurrentTumblerDigits[tumbler] = 9;
        }
        SetTumblerDigits();
    }

    //Update the visual display of the digits on the tumbler
    void SetTumblerDigits()
    {
        for (int i = 0; i < 5; i++)
        {
            if (ActiveDigits[i])
            {
                CurrentTumblerDigitText[i].text = CurrentTumblerDigits[i].ToString();
            }
        }
    }

    //Check the player's guess against the target
    void GuessCurrentTumbler()
    {
        //Figure out how far we are off. If the target is 6 and we guess 3, we're off by 3. If we guess 9, we're also off by 3
        //The player only gets the absolute error, no direction
        int OffBy = Mathf.Abs(CurrentTumblerDigits[CurrentTumbler] - CurrentTargetNumbers[CurrentTumbler]);
        if (OffBy == 0)
        {
            //Success! unlock this tumbler and move to the next, or we're done!
            Tumblers[CurrentTumbler].SetActive(false);
            //Move the lockpick 50 units right
            LockpickObject.DOAnchorPosX(LockpickObject.anchoredPosition.x + 50.0f, 0.3f);
            //LockpickObject.anchoredPosition += new Vector2(50.0f, 0);
            CurrentTumbler += 1;
            if (!ActiveDigits[CurrentTumbler])
            {
                OnSuccess();
            }
        }
        else
        {
            //Show a meter up to 8 - OffBy (higher equals closer)
            //It's 8 because we want to show at elast something if you're off by 9 (the max)
            int MeterValue = 8 - OffBy;
            Meter.fillAmount = MeterValue / 10.0f;
        }
    }

    public override string GetFriendlyName()
    {
        return "Anachronox";
    }
}
