using System.Collections;
using TMPro;
using UnityEngine;


public enum KeyDirection
{
    LEFT,
    RIGHT,
}

//Gothic's lockpick game was correctly guessing a sequence of four left/right key inputs (e.g. left left right left) - there's no visual feedback for the player, so the UI is pretty simple
public class GothicLockpickGame : LockpickGame
{

    Player thePlayer;

    //We generate a combination of directions randomly and store it here
    public KeyDirection[] currentCombo;

    //The index of the element in the current combo that we're at
    int comboIndex = 0;

    bool showTarget = false;

    bool ending = false;


    //UI
    public GameObject Panel;

    public TextMeshProUGUI targetText;

    public TextMeshProUGUI failureText;

    public TextMeshProUGUI sequenceText;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        currentCombo = new KeyDirection[4];

        SetupLock();
    }

    void SetupLock()
    {
        ending = false;
        sequenceText.text = "";
        showTarget = false;
        ShowHideTarget();
        failureText.gameObject.SetActive(false);

        //To show the player the goal sequence, we construct a string when we generate the directions below
        targetText.text = "";
        comboIndex = 0;
        //Generate valid combo. Valid combos are basically any four-direction sequence
        for (int i = 0; i < currentCombo.Length; i++)
        {
            //int Random.Range is max-exclusive, so this will generate 0 or 1
            //Cast it to a direction enum
            KeyDirection kd = (KeyDirection)Random.Range(0, 2);
            currentCombo[i] = kd;
            //Add the direction string to our target string
            if (kd == KeyDirection.LEFT)
            {
                targetText.text += "Left ";
            }
            else
            {
                targetText.text += "Right ";
            }
        }
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //Failure is definitely possible here, but all we do is tell the player to keep trying. In the real game, it breaks a lockpick but we don't have lockpicks
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //If the player succeeds, on the other hand, we end the game after a short delay (so they can appreciate it) and open the in-world lock
    public override void OnSuccess()
    {
        OpenLock();
        StartCoroutine(EndWithDelay());
    }

    //Wait five seconds before ending the game - also block input during that time
    IEnumerator EndWithDelay()
    {
        ending = true;
        yield return new WaitForSeconds(0.5f);
        EndLockpicking();
    }

    //Toggles the hint text for the player
    void ShowHideTarget()
    {
        targetText.gameObject.SetActive(showTarget);
    }

    //Wait for a bit, then hide the failure display
    IEnumerator HideFailureText()
    {
        yield return new WaitForSeconds(0.2f);
        failureText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        //If we're in the ending sequence, ignore further input
        if (ending)
        {
            return;
        }
        //For a right or left press, we check it against the correct direction expected by the combo. If it matches, we move forward. Otherwise we show the failure text
        //and put the player back at the beginning of the sequence. So it is possible to solve these locks by trial-and-error - in fact, it's the only way!
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentCombo[comboIndex] == KeyDirection.LEFT)
            {
                comboIndex += 1;
                sequenceText.text += "Left ";
                //Check if we're done
                if (comboIndex >= currentCombo.Length)
                {
                    OnSuccess();
                }
            }
            else
            {
                sequenceText.text = "";
                comboIndex = 0;
                //Error display
                failureText.gameObject.SetActive(true);
                HideFailureText();
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentCombo[comboIndex] == KeyDirection.RIGHT)
            {
                comboIndex += 1;
                sequenceText.text += "Right ";
                //Check if we're done
                if (comboIndex >= currentCombo.Length)
                {
                    OnSuccess();
                }
            }
            else
            {
                sequenceText.text = "";
                comboIndex = 0;
                //Error display
                failureText.gameObject.SetActive(true);
                HideFailureText();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            showTarget = !showTarget;
            ShowHideTarget();
        }
    }

    public override string GetFriendlyName()
    {
        return "Gothic";
    }
}
