using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//Wizardry 6's lockpicking game is essentially a slot machine with red or green spots on the reels. If the player manages to stop all the reels on green, they win
//The difficulty of the lock and the character's skill determine how much of the reel is green vs. red
public class Wizardry6LockpickGame : LockpickGame
{
    Player thePlayer;

    bool ending = false;
    
    //The difficulty is the percentage chance that a reel will be green
    //Remember that for the player to win, all reels must stop on green, so this is a pretty difficult game
    //A percentage of 75 is not as high as it seems because of combined probabilities
    public float difficulty = 0.75f;

    //Time between spins controls how often the images change color
    public float timeBetweenSpins = 0.05f;
    float spinTimer = 0.0f;

    //UI
    public GameObject Panel;

    //An array of the slot images that we recolor. We use the image component here because we're mainly changing color
    public Image[] Slots;

    //The text we display if the player succeeds
    public TextMeshProUGUI successText;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        //Reset the timer, hide the success text, and move us out of the ending state
        spinTimer = 0.0f;
        successText.gameObject.SetActive(false);
        ending = false;
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If we fail, just let the player know and exit
    public override void OnFailure()
    {
        successText.text = "Failure!";
        successText.gameObject.SetActive(true);
        StartCoroutine(EndWithDelay());
    }

    //If we succeed, open the in-world lock, let the player know, and exit
    public override void OnSuccess()
    {
        OpenLock();
        successText.text = "Success!";
        successText.gameObject.SetActive(true);
        StartCoroutine(EndWithDelay());
    }

    //This is a coroutine to ensure we don't end too abruptly and that the player can appreciate their success or failure
    IEnumerator EndWithDelay()
    {
        //Put us in the ending state so that player input doesn't happen
        ending = true;
        //Wait half a second
        yield return new WaitForSeconds(0.5f);
        EndLockpicking();
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        //If we're in the ending state, then ignore any further input
        if (ending)
        {
            return;
        }

        //If the player presses the button to stop the spinning, then let's see where they land
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //One way or another, we're ending
            ending = true;
            //Assume they succeeded
            bool success = true;
            foreach (Image i in Slots)
            {
                //If any reel is red, the player failed
                if (i.color == Color.red)
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                OnSuccess();
            }
            else
            {
                OnFailure();
            }
        }

        //Add elapsed time to the timer
        spinTimer += Time.deltaTime;
        //If it's greater than the time between spins, then set a new color for each reel
        if (spinTimer > timeBetweenSpins)
        {
            //Go through each slot and change the color depending on the difficulty
            foreach (Image i in Slots)
            {
                Color imageColor = Color.red;
                if (Random.value < difficulty)
                {
                    imageColor = Color.green;
                }

                i.color = imageColor;
            }
            //Reset the timer
            spinTimer = 0.0f;
        }

    }

    public override string GetFriendlyName()
    {
        return "Wizardry 6";
    }
}
