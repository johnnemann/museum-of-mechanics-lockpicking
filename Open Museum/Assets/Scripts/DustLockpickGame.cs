using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//An enum representing the four cardinal directions that a combo can include
public enum DustDirections
{
    UP,
    RIGHT,
    DOWN,
    LEFT,
    NONE,
}

//The lockpicking game in Dust is an action game - a prompt appears with a countdown, and the player must press the four face buttons on the controller
//in response to the game's prompts, and get through the whole sequence before time is up.
public class DustLockpickGame : LockpickGame
{

    Player thePlayer;

    //Transforms for the various moving parts of the UI. We use transform components because we're mostly interested in moving these around
    //The selector is the thing that points at the button to push
    public Transform selectorTransform;

    //The "moving part" is the sliding bit that moves as we progress through the lock
    public Transform movingPartTransform;

    //Store the start and end locations so that we know when we're done and can also reset to the beginning
    public Vector3 movingPartStartLocation;
    public Vector3 movingPartEndLocation;

    //This is the combo we've generated that the player is trying to match
    DustDirections[] currentCombo;

    //A tunable number for how many moves go into a generated combo
    public int comboSize = 7;

    //The tumbler object we're currently unlocking
    int currentTumbler = 0;

    //Tunable number of seconds that the player has to complete the combo before failure
    public float countdownTime = 15f;
    //To track that time
    float currentCountdown;
    //A bool to see if we're in the failure state so we can eat input and not let the player succeed
    bool failing = false;

    //UI
    //The panel object containing the game UI
    public GameObject Panel;
    //Where the tumblers should start at, positionally
    public float tumblerStartValue = 0.3f;
    //An array of the tumbler images, to animate
    public Image[] tumblers;
    //The images that we show as prompts to the player. They are shown/hidden depending on the combo values
    [Tooltip("This is indexed by the directional enum so they need to be in that order (UP, RIGHT, DOWN, LEFT)")]
    public GameObject[] directionalImages;

    //Store the original color of the selector so that we can reset it after an error
    public Color selectorOriginalColor;

    //When the player screws up the combo we flash the selector a color to indicate (red probably)
    public Color selectorErrorColor;

    //This function generates a random combination of directional prompts of comboSize length
    void GenerateCombo()
    {
        currentCombo = new DustDirections[comboSize];
        //Loop the number of directions we need
        for (int i = 0; i < comboSize; i++)
        {
            //Generate a random number between 0 and 4 (exclusive) and cast it to our Direction enum value
            DustDirections d = (DustDirections)Random.Range(0, 4);
            currentCombo[i] = d;
        }
    }

    //This sets the lock back to its initial values, for failure or at the beginning of the minigame
    void ResetLock()
    {
        //Start with the first tumbler
        currentTumbler = 0;
        //Rotate the selector to its start position and display the correct starting prompt
        selectorTransform.rotation = Quaternion.AngleAxis(-90 * (int)currentCombo[currentTumbler], Vector3.forward);
        //Display correct prompt:
        //First, hide all the prompts
        for (int i = 0; i < directionalImages.Length; i++)
        {
            directionalImages[i].SetActive(false);
        }
        //Then, index the directional images array by the current direction we need to show and display corresponding image
        directionalImages[(int)currentCombo[currentTumbler]].SetActive(true);

        //move the moving piece back to its start location
        movingPartTransform.localPosition = movingPartStartLocation;

        //reset the tumblers
        for (int i = 0; i < tumblers.Length; i++)
        {
            tumblers[i].fillAmount = tumblerStartValue;
        }
        
    }

    //This takes care of moving to the next tumbler in the set, by incrementing the counter and also "extending" the current tumbler by using Unity's filled sprite feature
    void NextTumbler()
    {
        tumblers[currentTumbler].fillAmount = 1.0f;
        currentTumbler += 1;

    }

    //Called when the player starts the game
    public override void BeginLockpicking(Player player)
    {
        //Set the panel active and freeze the player in place
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        //Generate the combo for this game, reset the lock to starting parameters, and clear the countdown and failure states
        GenerateCombo();
        ResetLock();
        currentCountdown = 0;
        failing = false;
    }

    //Ends lockpicking and puts the player back in the museum
    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If we fail the game, we just get booted out to the museum
    public override void OnFailure()
    {
        Debug.Log("Failed!");
        EndLockpicking();
    }

    //If we succeed, open the in-world lock and then end the minigame
    public override void OnSuccess()
    {
        Debug.Log("Succcess!");
        OpenLock();
        EndLockpicking();
    }


    void Update()
    {
        //get input and deal with it

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //Here we figure out the input direction that the player has pressed each frame, if any
        DustDirections inputDirection = DustDirections.NONE;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            inputDirection = DustDirections.LEFT;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            inputDirection = DustDirections.RIGHT;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            inputDirection = DustDirections.UP;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            inputDirection = DustDirections.DOWN;
        }

        //Here, test that inputted direction against the current combo value. If they match, then either we succeeded (if it's the last tumbler), or we move to the next tumbler
        //If tumbler size and combo size ever mismatch here, we have a problem - probably should make comboSize just the tumbler count?
        if (inputDirection == currentCombo[currentTumbler])
        {
            if (currentTumbler >= tumblers.Length -1)
            {
                OnSuccess();
            }
            else
            {
                NextTumbler();
                //rotate directional piece to the correct orientation
                //TODO: tween this instead?
                selectorTransform.rotation = Quaternion.AngleAxis(-90 * (int)currentCombo[currentTumbler], Vector3.forward);
                //display correct prompt
                //Hide all the prompts
                for (int i = 0; i < directionalImages.Length; i++)
                {
                    directionalImages[i].SetActive(false);
                }
                //Index the directional images array by the current direction we need to show and display corresponding image
                directionalImages[(int)currentCombo[currentTumbler]].SetActive(true);
            }
        }
        //Otherwise, if the player has pressed a direction but it's not the right one, then we let them know and don't proceed forward
        else if(inputDirection != DustDirections.NONE)
        {
            //What do we do if they hit the wrong button? I don't know what happens in the game; does it fail or just not move forward?
            //The latter feels better to me, so that's what we'll do. Might not be accurate
            //Blink red so the player knows they fucked up
            //We use DOTween here to animate a nice blinking color
            //OPENSOURCE: remove dotween
            selectorTransform.GetComponent<Image>().DOColor(selectorErrorColor, 0.1f).SetLoops(5, LoopType.Yoyo).OnComplete(delegate { selectorTransform.GetComponent<Image>().DOColor(selectorOriginalColor, 0.1f); });
       
        }

        //Update time and move the moveable piece
        currentCountdown += Time.deltaTime;
        //If we've just failed, then let the player know and put us in the failing state
        if (currentCountdown >= countdownTime && !failing)
        {
            failing = true;
            //Blink the part red so that the player knows they failed because it reached the end. We're just using the same colors as the selector here, maybe that will change
            //OPENSOURCE: remove dotween
            movingPartTransform.GetComponent<Image>().DOColor(selectorErrorColor, 0.1f).SetLoops(5, LoopType.Yoyo).OnComplete(delegate { selectorTransform.GetComponent<Image>().color = selectorOriginalColor; OnFailure(); });
            
        }
        else
        {
            //Otherwise, move the piece a little closer to its end position
            movingPartTransform.localPosition = Vector3.Lerp(movingPartStartLocation, movingPartEndLocation, currentCountdown / countdownTime);
        }
        
    }

    public override string GetFriendlyName()
    {
        return "Dust: An Elysian Tale";
    }
}
