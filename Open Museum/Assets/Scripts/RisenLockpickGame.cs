using UnityEngine;
using System.Collections;
using DG.Tweening;

//A note:
//The non-open-source version of the Museum uses a package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//The lockpicking game in Risen is about hitting the tumblers in a lock in a particular order. If the player hits a tumbler out of sequence, they all fall.
//The sequence has to be solved by trial-and-error
public class RisenLockpickGame : LockpickGame
{
    //This is a list of all the valid sequences for this puzzle. Because of the mechanics of the game, you can never cross a higher number in search of the next number in
    //sequence, but you can cross lower numbers. Also the first tumbler can be anywhere (because until you hit it, the game will just be in default state of all tumblers down)
    //1, 2, 3, 4 is valid but trivial and kind of a bullshit arrangement for exploring how the game works
    int[][] validTumblerArrangements = 
        new int[][] { 
            //new int[] { 1, 2, 3, 4 },
            new int[] { 2, 1, 3, 4 },
            new int[] { 3, 2, 1, 4 },
            new int[] { 4, 3, 2, 1 },
            new int[] { 4, 3, 1, 2 },
            new int[] { 4, 2, 1, 3 },
            new int[] { 3, 1, 2, 4 },
            new int[] { 4, 1, 2, 3 }
        };
    Player thePlayer;

    //The tumbler we're on
    int currentTumbler;

    //One of the valid sequences from above, chosen at random on game start
    int[] tumblerOrder;

    int tumblerProgress;

    //The player can toggle hints on
    bool showTarget = false;

    //Track whether each tumbler is solved or not
    bool[] tumblerStatus;

    //UI
    public GameObject Panel;

    //The lockpick object so we can move it around
    public Transform lockpick;

    //The tumblers
    public RectTransform[] tumblers;

    //The numbers that show up over the tumblers for the player hints
    public TMPro.TextMeshProUGUI[] keyDigits;

    //These are values for setting the position of the tumblers in solved (up) and unsolved (down) states
    public float tumblerUpYValue;

    public float tumblerDownYValue;

    //When the pick is under it, raise it up slightly
    public float tumblerActiveYValue;

    //This is a list of all the positions the lockpick is in when it moves across the space. It's easiest to just set this by hand in the editor
    public Vector3[] lockpickPositions;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupLock();
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //There's not really a "fail out" mode, the game just resets and you get to keep trying to solve it.
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //If the player succeeds, open the lock and end the minigame
    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
        EndLockpicking();
    }

    public void SetupLock()
    {
        showTarget = false;
        currentTumbler = 0;

        //Reset lockpick and tumbler positions
        ResetTumblers();
        lockpick.localPosition = lockpickPositions[0];

        //generate sequence for tumblers - pick from list of valid sequences
        int randomArrangement = Random.Range(0, validTumblerArrangements.Length);
        tumblerOrder = validTumblerArrangements[randomArrangement];

        tumblerProgress = 1;
        ShowHideTarget();

    }

    //This shows or hides the hint digits above the tumblers
    void ShowHideTarget()
    {
        for(int i=0;i<keyDigits.Length;i++)
        {
            keyDigits[i].gameObject.SetActive(showTarget);
            keyDigits[i].text = tumblerOrder[i].ToString();
        }
    }

    void Awake()
    {
        tumblerStatus = new bool[tumblers.Length];
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            showTarget = !showTarget;
            ShowHideTarget();
        }

        //If the player moves right or left, we increment or decrement the current tumbler, checking our bounds first
        //Then we move the lockpick to the appropriate place and check to see whether or not it was the right move
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentTumbler < tumblers.Length - 1)
            {
                currentTumbler += 1;
                MoveLockpick();
                CheckProgress();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentTumbler > 0)
            {
                currentTumbler -= 1;
                MoveLockpick();
                CheckProgress();
            }
        }

    }

    IEnumerator SuccessDelay()
    {
        //Do a delay here so that the player can see the tumblers move, etc
        yield return new WaitForSeconds(0.5f);
        OnSuccess();
    }

    //All this does is move the lockpick to the appropriate position depending on the current tumbler index
    void MoveLockpick()
    {
        //TODO: Tween this
        lockpick.localPosition = lockpickPositions[currentTumbler];
    }

    //This checks to see whether the last move we made was the right one, and if so if we're at the end
    void CheckProgress()
    {
        if (tumblerOrder[currentTumbler] == tumblerProgress)
        {
            //We got the right one, so increment the one we're looking for and raise the current tumbler
            tumblerProgress += 1;
            RaiseCurrentTumbler();
            if (tumblerProgress > 4)
            {
                //Success!
                StartCoroutine(SuccessDelay());
            }
        }
        else if (tumblerStatus[currentTumbler])
        {
            //This one is already raised, so just go on
        }
        else
        {
            //We didn't get the right one, so make all tumblers fall (but animate the current one up then down)
            ResetTumblers();
        }
    }

    void RaiseCurrentTumbler()
    {
        //Animate tumbler movement up to y value
        tumblers[currentTumbler].DOLocalMoveY(tumblerUpYValue, 0.1f).SetEase(Ease.OutExpo);
        //Set the tumbler status so that we know it's raised
        tumblerStatus[currentTumbler] = true;
        //Set the position of all the others down (because we animated them up when the lockpick passed by)
        for (int i = 0; i < tumblers.Length; i++)
        {
            if (!tumblerStatus[i])
            {
                tumblers[i].DOLocalMoveY(tumblerDownYValue, 0.01f);
            }
        }
    }

    //Set the tumbler positions and statuses back to the starting state
    void ResetTumblers()
    {
        tumblerProgress = 1;
        //Drop all tumblers back to lowered position
        foreach (Transform t in tumblers)
        {
            t.DOLocalMoveY(tumblerDownYValue, 0.1f).SetEase(Ease.InOutElastic).OnComplete(delegate () { tumblers[currentTumbler].DOLocalMoveY(tumblerActiveYValue, 0.01f); }); ;
        }
        //Reset their status in the array
        for (int i = 0; i < tumblerStatus.Length; i++)
        {
            tumblerStatus[i] = false;
        }
    }

    public override string GetFriendlyName()
    {
        return "Risen 2 + 3";
    }
}
