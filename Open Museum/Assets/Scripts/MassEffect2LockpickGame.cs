using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//Mass Effect 2's lockpicking game was essentially a game of Memory - the player flips over a tile and then has to find the matching tile from the rest of the set
//There's a timer to add tension
public class MassEffect2LockpickGame : LockpickGame
{
    Player thePlayer;

    //Index of the current pair the player is focusing on and the current Sprite that's flipped
    int currentPair = 0;
    Sprite currentSprite = null;

    //The random sprites populating the game
    List<Sprite> selectedSprites;

    //Tunable value for how long the player has to finish the game
    public float countdownTime = 15.0f;

    float currentCountdown = 0f;

    bool failed = false;
    bool succeeded = false;

    //UI
    public GameObject Panel;

    //The text to display the timer value
    public TMPro.TextMeshProUGUI timerText;

    //The sprite we show when a node is hidden
    public Sprite hiddenSprite;

    //All of the sprites we use for symbols
    public List<Sprite> symbolSprites;

    //The nodes placed in the UI that the player can click on. See the MassEffect2Node class for more info
    public List<MassEffect2Node> nodes;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupLock();
    }

    void SetupLock()
    {
        //Randomly assign sprites to nodes, assigning each symbol exactly twice
        //Do this by selecting a number of sprites to use, half of the number of nodes, but add each sprite twice
        selectedSprites.Clear();
        List<Sprite> shuffledList = ShuffleSpriteList(symbolSprites);
        for (int i = 0; i < nodes.Count / 2; i++)
        {
            selectedSprites.Add(shuffledList[i]);
            selectedSprites.Add(shuffledList[i]);
        }
        //And then shuffling that list, so that the sprites are not next to each other
        selectedSprites = ShuffleSpriteList(selectedSprites);

        int j = 0;
        foreach (MassEffect2Node node in nodes)
        {
            //And then assigning each sprite to a node
            node.flippedSprite = selectedSprites[j];
            node.hiddenSprite = hiddenSprite;
            //Reset nodes to background sprite, initialize it
            node.HideSprite();
            node.lockpickGame = this;
            j++;
        }
        currentCountdown = countdownTime;
        currentPair = 0;
        currentSprite = null;
        failed = false;
        succeeded = false;
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If the player runs out of time, show them that and then end the game after a short delay
    public override void OnFailure()
    {
        Debug.Log("Failed!");
        timerText.text = "FAILED!";
        failed = true;
        StartCoroutine(EndWithDelay());
    }

    //A coroutine so that the player can see the results of their actions before the game boots them out. Wait a half second then end the game
    IEnumerator EndWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        EndLockpicking();
    }

    //If the player succeeds, we tell them so, open the in-world lock, then end the game after a short delay
    public override void OnSuccess()
    {
        succeeded = true;
        timerText.text = "SUCCESS!";
        OpenLock();
        StartCoroutine(EndWithDelay());
    }

    //This gets called from the node objects because it's easier to do this logic here in the main game
    public void SpriteClicked(MassEffect2Node node)
    {
        //We're comparing sprites here, which isn't really a great idea. A better one would be to make a data structure that paired a sprite to an int ID
        //and assign one of those to each node instead of just a sprite, then compare the IDs
        //if current sprite does not exist, set it
        if (currentSprite == null)
        {
            currentSprite = node.flippedSprite;
        }
        //if it does, compare them. If they're not the same, fail. If they are the same, continue
        else
        {
            if (currentSprite == node.flippedSprite)
            {
                currentPair += 1;
                currentSprite = null;
            }
            else
            {
                //FAILURE
                OnFailure();
            }
        }

        //Check if we've completed all pairs - if so, the player succeeds
        if (currentPair >= nodes.Count / 2)
        {
            OnSuccess();
        }
    }

    //On awake we just need to initialize our list of sprites
    void Awake()
    {
        selectedSprites = new List<Sprite>();
    }

    //Because most input is just hitting buttons in the UI, there's not a ton to worry about here
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //Since the nodes track most player input, the only thing we need to worry about here is the timer
        if (!failed && !succeeded)
        {
            currentCountdown -= Time.deltaTime;
            timerText.text = currentCountdown.ToString("F2");
            //If the time has run out, the player fails
            if (currentCountdown <= 0f)
            {
                OnFailure();
            }
        }
    }

    //This is a function to take a list of sprites and shuffle it to re-order things in a good random order
    List<Sprite> ShuffleSpriteList(List<Sprite> list)
    {
        //Make a copy of our initial list to use
        List<Sprite> ShuffledList = list;
        //Go through the list one element at a time, and swap the current element with a random one somewhere in the list
        for (int i = 0; i < ShuffledList.Count; i++)
        {
            Sprite temp = ShuffledList[i];
            int randomIndex = Random.Range(i, ShuffledList.Count);
            ShuffledList[i] = ShuffledList[randomIndex];
            ShuffledList[randomIndex] = temp;
        }

        return ShuffledList;
    }

    public override string GetFriendlyName()
    {
        return "Mass Effect 2";
    }
}
