using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//A class that defines a "puzzle" - a puzzle is one particular layout of plates and rods
[System.Serializable]
public class LockPuzzle
{
    //The object in the scene to activate/deactivate for this particular puzzle
    public GameObject puzzleObject;
    //All the plate objects in this puzzle
    public List<KingsHeirPlateUI> plates;
    //The hint numbers - these are set up manually and are activated/deactivated when the T key is pressed
    public TextMeshProUGUI[] hintTexts;
}


//In King's Heir, the lockpicking takes the form of a particular puzzle where the player must retract a set of rods in order - each rod retracted lets another one be,
//and so forth. There's just one puzzle in the game but I implemented three here, which show up randomly
public class KingsHeirLockpickGame : LockpickGame
{
    Player thePlayer;

    //Index of the current puzzle in our list of puzzles
    int currentPuzzle = 0;

    //All the possible puzzles
    public List<LockPuzzle> puzzles;

    //When a plate is pressed, if it's valid, we add the newly-unlocked plates to this set.
    //This enables us to have multiple valid next plates, and they will remain valid even if they're left for later
    List<int> unlockedPlateSet;

    //UI

    //The panel containing the lockpick game
    public GameObject Panel;

    //Whether or not we are showing the hint text
    bool showHints = false;

    //Called when the player interacts with the lock
    public override void BeginLockpicking(Player player)
    {
        unlockedPlateSet = new List<int>();
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupLock();
    }

    //Get the plate object, given its numerical ID
    KingsHeirPlateUI GetPlateFromID(int id)
    {
        KingsHeirPlateUI plate = puzzles[currentPuzzle].plates.Find(x => x.plateID == id);
        return plate;
    }

    //Called to initialize the lock back to playable status
    void SetupLock()
    {
        currentPuzzle = Random.Range(0, puzzles.Count);

        LockPuzzle current = puzzles[currentPuzzle];

        current.puzzleObject.SetActive(true);

        foreach (KingsHeirPlateUI plate in current.plates)
        {
            plate.ResetLock(true);
            //set up OnClick for each plate to tell us which index was clicked
            Button buttonComponent = plate.plateButton;
            buttonComponent?.onClick.AddListener(delegate { PlateClicked(plate.plateID); });
        }
        //Randomize selected plate so the player doesn't know the first one to start with
        int randomPlate = Random.Range(0, current.plates.Count);
        EventSystem.current.SetSelectedGameObject(current.plates[randomPlate].plate);

        //hide all the hint numbers
        foreach (TextMeshProUGUI tmp in current.hintTexts)
        {
            tmp.gameObject.SetActive(false);
        }

        unlockedPlateSet.Clear();
    }

    //Stops the game and re-activates the player
    public override void EndLockpicking()
    {
        puzzles[currentPuzzle].puzzleObject.SetActive(false);
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();

    }

    //Not implemented - we don't really "fail" this game, we just reset to starting state
    //That COULD have been implemented in this function, but instead it's just handled as part of the gameplay flow
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //If we succeed, open the in-world lock and end the game
    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    //A simple coroutine to delay for a little bit before ending the puzzle - this both decreases the disorientation of finishing
    //and makes sure that we don't double up input in-game and out-of-game (for instance, the final controller button press might otherwise be read as
    //triggering the last plate AND interacting again with the lock, putting us back into the puzzle
    IEnumerator WaitForSuccess()
    {
        yield return new WaitForSeconds(1.3f);
        OnSuccess();
    }

    //Called from the PlateUI object when the player clicks (or activates with controller)
    void PlateClicked(int ID)
    {
        //Get the plate
        KingsHeirPlateUI plate = GetPlateFromID(ID);

        //Check to see if we can press this plate - if every required plate has been unlocked then we can, otherwise we fail
        bool unlocked = true;
        foreach (int i in plate.requiredPlates)
        {
            if (!unlockedPlateSet.Contains(i))
            {
                unlocked = false;
                break;
            }

          
        }
        
        //If we succeeded up above
        if (unlocked)
        {
            //Add this plate to the unlocked set
            unlockedPlateSet.Add(plate.plateID);

            //"Retract" the bars
            plate.Retract();

            //If this plate is the last one, we're done!
            if (plate.lastPlate)
            {
                //delay to give the player a sense of triumph and clear UI buffer
                StartCoroutine(WaitForSuccess());
            }
        }
        //Oops, this wasn't valid
        else
        {
            //Reset the whole puzzle to the starting state
            foreach (KingsHeirPlateUI p in puzzles[currentPuzzle].plates)
            {
                p.ResetLock(false);
            }
            //Clear the list of unlocked plates
            unlockedPlateSet.Clear();
        }
    }

    //Called to toggle hints on or off
    void ToggleHints()
    {
        showHints = !showHints;
        foreach (TextMeshProUGUI tmp in puzzles[currentPuzzle].hintTexts)
        {
            tmp.gameObject.SetActive(showHints);
        }
    }

    //Handles player input - very simple for this game since most input is hitting buttons in the UI
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleHints();
        }
    }


    //TODO: can we procedurally generate these puzzles? It doesn't seem impossible, but it is tricky enough that I haven't taken a stab yet.
    void GenerateLock()
    {
        //Layout N plates, between 5-10, probably
        //Number them sequentially
        //Extend a line (or two) from the last plate
        //Counting down, extend a line or two from each subsequent plate to intersect the previous line(s)
        //Choose distance for lines - just past the target line we want to lock
        //Once all lines are made, you have a valid puzzle!
        //UNLESS - it's impossible to reach a line without crossing another one - that might be OK, because it's all unlocked there
        //But we could cross a plate, which would be bad, we'd need to move
        //It's possible we've placed a plate badly, and there's nowhere for the lines to cross - so our plate placement needs to take that into account somehow
        //Also these puzzles will be very simple, because they don't make strong use of multiple lines/orderings - we could maybe choose two/N plates to contact?

        //These puzzles are not hard to generate on paper - my approach is basically to make a bunch of circles, number them, and then work in order to connect them with lines
        //But as a human it's easier to see when the layout is bad and needs changing, and also to take advantage of multiple line crossings and emergent properties

    }

    public override string GetFriendlyName()
    {
        return "King's Heir: Rise to the Throne";
    }
}
