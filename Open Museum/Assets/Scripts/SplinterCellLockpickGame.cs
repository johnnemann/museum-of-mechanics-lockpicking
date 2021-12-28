using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

//An enum for the four possible directions in a lockpicking stage
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

//In Splinter Cell, the lockpicking consists of guessing the correct directional prompt at each stage and pressing the corresponding button. There's no skill or chance of failure
//Most of the interest is in the fact that the game uses a cutaway 3D lock to display the player's progress and it looks very fancy
public class SplinterCellLockpickGame : LockpickGame
{
    Player thePlayer;

    //An animator for the 3d lockpick
    //public Animator lockpickAnimator;

    //The position of the lockpick at each stage
    public List<Vector3> lockpickPositions;

    //The starting rotation of the lockpick
    public Vector3 LockpickRotation;

    //The transform of the lockpick so that we can move it around
    public Transform lockpickTransform;

    //Transforms for the tumblers in the lock, for moving them around
    public Transform[] tumblers;

    //Track whether we're in the middle of picking and don't let the player do input if we are
    bool picking = false;

    //UI Stuff
    public GameObject Panel;

    //The render camera here points at the 3d model of the lock for the purposes of displaying it on the UI via a render texture
    public Camera GameRenderCamera;

    //The current correct direction that the player is guessing, the number of times they need to guess it, and the index of the tumbler they're working on
    Direction currentDirection;
    int currentTargetCount;
    int currentCount;
    int currentTumbler;

    public override void BeginLockpicking(Player player)
    {
        //Activate the render camera and the UI panel, and lock the player in place
        GameRenderCamera.gameObject.SetActive(true);
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        //Choose the initial goal direction and number of taps
        ChooseCurrentDirection();
        currentTumbler = 0;
        lockpickTransform.localPosition = lockpickPositions[currentTumbler];

    }

    public override void EndLockpicking()
    {
        GameRenderCamera.gameObject.SetActive(false);
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //This function randomly chooses the goal direction for the current stage, as well as a goal number
    public void ChooseCurrentDirection()
    {
        //Which direction button to hit to successfully pick
        int direction = UnityEngine.Random.Range(0, 4);
        currentDirection = (Direction)direction;
        //How many times we need to try before it lets us through
        currentTargetCount = UnityEngine.Random.Range(1, 6);

        currentCount = 0;
    }

    //It is not possible to fail this game
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //If the player succeeds, open the in-world lock and end the minigame
    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //If we are in the middle of picking (specifically the animations) don't let the player continue to input button presses
        if (picking)
        {
            return;
        }

        //If the player presses the correct direction, then we increment their count. If not, nothing happens (except a sound)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) && currentDirection == Direction.Up)
        {
            PickCurrent();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) && currentDirection == Direction.Down)
        {
            PickCurrent();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) && currentDirection == Direction.Left)
        {
            PickCurrent();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) && currentDirection == Direction.Right)
        {
            PickCurrent();
        }
    }

    //This simply sets the picking flag to false, so that we read player input again
    void EndPicking()
    {
        picking = false;
    }

    void PickCurrent()
    {
        picking = true;
        currentCount += 1;
        //Bounce the lockpick up and back down, using DOTween
        lockpickTransform.DORotate(LockpickRotation, 0.4f).SetLoops(1, LoopType.Yoyo).SetEase(Ease.InOutBounce).OnComplete(EndPicking);

        //If we've not completed the correct number of taps yet, bounce the tumbler up but settle it back down
        if (currentCount < currentTargetCount)
        {
            tumblers[currentTumbler].DOLocalMoveY(0.3f, 0.4f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutBounce).OnComplete(EndPicking);
        }
        //If we have, then send the tumbler up and lock it there. Then, when that's done, check to see if we are done picking
        else
        {
            tumblers[currentTumbler].DOLocalMoveY(0.3f, 0.4f).SetEase(Ease.InOutBounce).OnComplete(CheckProgress);
        }
    }

    //Check to see if we've succeeded
    void CheckProgress()
    {
        EndPicking();
        //If we've done enough taps
        if (currentCount >= currentTargetCount)
        {
            //Proceed to next tumbler, unless we're done
            currentTumbler += 1;
            //Currently we only make the player go through 3 tumblers (out of 5 on the model), because it's kind of tedious. This should probably be tunable, though
            if (currentTumbler >= 3)
            {
                OnSuccess();
            }
            else
            {
                //Choose a new direction and goal number, and move the lockpick to the next tumbler
                ChooseCurrentDirection();
                lockpickTransform.localPosition = lockpickPositions[currentTumbler];
            }
        }
    }

    public override string GetFriendlyName()
    {
        return "Splinter Cell";
    }
}
