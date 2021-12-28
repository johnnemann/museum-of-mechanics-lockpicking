using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game



//A shape is a simple thing that has an ID (for easy comparison) and a sprite for the visual representation. A single pick has two shapes.
[System.Serializable]
public class Shape
{
    public Sprite sprite;
    public int id;
}

//Hillsfar's game is a matching game - the player has to find the pick that matches the negative space of each tumbler in the lock
//To complicate matters, each pick has two ends, one of which is upside down. AND, there's a timer. So finding the right matches for
//three tumblers in a row can be challenging
public class HillsfarLockpickGame : LockpickGame
{
    Player thePlayer;

    //An index 0-9, but depending on the value of the corresponding flipped value
    int selectedLockpickPair = 0;

    //The tumbler the player is working on
    int currentTumbler;

    //Whether the current lockpick is flipped or not
    bool flipped;

    //A set of tumbler IDs
    int[] tumblers = new int[3];

    //UI Stuff
    public GameObject Panel;

    //This is the number of different types of lockpick/tumbler there are. Hillsfar seems to have ten pairs, so twenty total.
    public int variationCount = 20;

    //The amount of time the player has to complete the entire lockpicking. Hillsfar is VERY agressive with this, we may want to be a little softer
    public float PickTimer = 20f;
    float CurrentPickTimer;

    bool inFailureState;

    //Add the starting positions of the tumblers here - the most foolproof way of making sure they start where they should
    public Vector3[] TumblerStartingPositions;

    //Sprites for tumblers and lockpicks (20 tumblers, 10 lockpicks)
    public List<Shape> tumblerSprites;
    public List<Shape> pickSprites;

    //Image for current active lockpick, up near the lock
    public Image activeLockpickA;
    public Image activeLockpickB;

    //Gameobjects for the lockpick
    public GameObject[] LockpickObjects;

    //Images for lockpick choosing menu - one for each pick end
    public Image[] lockpicks;

    //Images for selector under each lockpick
    public Image[] selectors;

    //The set of three images that we will put sprites on when we generate the puzzle
    public Image[] tumblerImages;

    //For the timer fuse
    public Image FuseImage;

    //The panel we display on failure
    public GameObject FailurePanel;


    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        ResetTumblerPositions();
        CurrentPickTimer = PickTimer;
        inFailureState = false;
        currentTumbler = 0;

        SetupInitialUI();
    }

    void SetupInitialUI()
    {
        selectedLockpickPair = 0;
        //randomize tumblers and set up the images and object IDs
        List<Shape> shuffledTumblers = ShuffleShapeList(tumblerSprites);
        for (int i = 0; i < tumblerImages.Length; i++)
        {
            tumblerImages[i].sprite = shuffledTumblers[i].sprite;
            tumblers[i] = shuffledTumblers[i].id;
        }
        //randomize picks

        List<Shape> shuffledPicks = ShuffleShapeList(pickSprites);
        //This is interesting because we can have a mismatch here - we have 19 pick images but 20 slots to fill! So we need to redo one, and it's easiest to just redo the first
        //(although this code redoes the first N < double, but for us N should be 1)
        for (int i = 0; i < lockpicks.Length; i++)
        {
            if (i < shuffledPicks.Count)
            {
                lockpicks[i].sprite = shuffledPicks[i].sprite;
                lockpicks[i].GetComponent<HillsfarPick>().shape = shuffledPicks[i];
            }
            else
            {
                lockpicks[i].sprite = shuffledPicks[i - shuffledPicks.Count].sprite;
                lockpicks[i].GetComponent<HillsfarPick>().shape = shuffledPicks[i - shuffledPicks.Count];
            }
        }
        //disable selectors except for the first
        for (int i = 1; i < selectors.Length; i++)
        {
            selectors[i].gameObject.SetActive(false);
        }
        UpdateActiveLockpick();
    }

    public override void OnFailure()
    {
        //Display failure dialog
        FailurePanel.SetActive(true);
        inFailureState = true;
    }

    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    //This is what actually ends the game if the player fails
    public void AfterFailure()
    {
        FailurePanel.SetActive(false);
        ResetTumblerPositions();
        EndLockpicking();
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    void Update()
    {
        //Get input and respond accordingly
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //Don't need to do anything else in here if we're waiting on the player to acknowledge their abject failure
        if (inFailureState)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AfterFailure();
            }
            return;
        }

        //"Flipping" changes which end of the current lockpick is active
        if (Input.GetKeyDown(KeyCode.F))
        {
            FlipSelectedLockpick();
        }

        //"Picking" tests the current active pick and end against the current tumbler
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Pick();
        }

        //The player selects from lockpicks by navigating a grid of all available lockpicks - they can move up, down, left, or right to go through the grid
        //movement should move up, down, left, right from current slot, not wrapping at edges
        //Hillsfar has two rows of four and a row of two, which seems annoying. I think we'll do two rows of five
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (selectedLockpickPair != 0 && selectedLockpickPair != 5)
            {
                SetSelectedLockpickPair(selectedLockpickPair - 1);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (selectedLockpickPair != 9 && selectedLockpickPair != 4)
            {
                SetSelectedLockpickPair(selectedLockpickPair + 1);
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (selectedLockpickPair > 4)
            {
                SetSelectedLockpickPair(selectedLockpickPair - 5);
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (selectedLockpickPair <= 4)
            {
                SetSelectedLockpickPair(selectedLockpickPair + 5);
            }
        }

        //update timer and check if we've failed due to time limit
        CurrentPickTimer -= Time.deltaTime;
        if (CurrentPickTimer <= 0)
        {
            //Failure!
            OnFailure();
        }
        else
        {
            float percentage = CurrentPickTimer / PickTimer;
            FuseImage.fillAmount = percentage;
        }
    }

    void Pick()
    {
        //Test selected lockpick against current tumbler
        if (lockpicks[GetCurrentLockpickIndex()].GetComponent<HillsfarPick>().shape.id == tumblers[currentTumbler])
        {
            //Animate the tumbler
            tumblerImages[currentTumbler].transform.DOLocalMoveY(transform.localPosition.y - 160, 0.5f);
            //A success. Move on to the next tumbler, or, if we've reached the end, unlock the lock.
            currentTumbler += 1;
            if (currentTumbler >= tumblerImages.Length)
            {
                ResetTumblerPositions();
                OnSuccess();
            }
            
        }
        else
        {
            OnFailure();
            //A failure!
        }
    }

    //Go through all the tumblers and reset them to the top for a new game
    void ResetTumblerPositions()
    {
        for (int i = 0; i < tumblerImages.Length; i++)
        {
            DOTween.CompleteAll();
            //Reset the tumbler positions
            tumblerImages[i].transform.localPosition = TumblerStartingPositions[i];
        }
    }

    int GetCurrentLockpickIndex()
    {
        //The index of the lockpick pair should be 0-9, but the corresponding tumbler/pick sprites are actually (index * 2) and (index * 2) + 1
        return selectedLockpickPair * 2 + (flipped ? 1 : 0);
    }

    void SetSelectedLockpickPair(int index)
    {
        flipped = false;
        //Lockpick Pairs are a set of two lockpicks that can be flipped to access the other one.
        //Set the arrow selector to visible on the selected lockpick
        selectors[selectedLockpickPair].gameObject.SetActive(false);
        selectors[index].gameObject.SetActive(true);

        //Set index
        selectedLockpickPair = index;

        //Set current lockpick sprite to our sprite (correctly flipped)
        UpdateActiveLockpick();

    }

    //This updates the visual state of the active lockpick, which is shown larger, up near the lock
    private void UpdateActiveLockpick()
    {
        //We want the current TWO indices for the lockpicks - we need to set both ends of the lockpick
        int pickA = GetCurrentLockpickIndex();
        int pickB = selectedLockpickPair * 2 + (flipped ? 0 : 1);

        //There might be a better way to do this, if we shuffled the list in-place we could use the index instead of grabbing the sprite from the lockpick?
        activeLockpickA.sprite = lockpicks[pickA].sprite;
        activeLockpickB.sprite = lockpicks[pickB].sprite;
    }

    //This just sets our flipped flag and re-does the visuals to indicate that it's turned around
    void FlipSelectedLockpick()
    {
        flipped = !flipped;
        UpdateActiveLockpick();
    }

    //A function to shuffle the shapes to get random ones - it just goes through and swaps each element for another random one in the list
    List<Shape> ShuffleShapeList(List<Shape> list)
    {
        List<Shape> ShuffledList = list;
        for (int i = 0; i < ShuffledList.Count; i++)
        {
            Shape temp = ShuffledList[i];
            int randomIndex = Random.Range(i, ShuffledList.Count);
            ShuffledList[i] = ShuffledList[randomIndex];
            ShuffledList[randomIndex] = temp;
        }

        return ShuffledList;
    }

    public override string GetFriendlyName()
    {
        return "Hillsfar";
    }
}
