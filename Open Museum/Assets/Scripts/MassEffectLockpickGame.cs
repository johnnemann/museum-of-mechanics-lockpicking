using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In Mass Effect, the lockpicking game involves the player steering a little triangle towards the center of a circle, dodging obstacles by rotating
//around the circle and moving forward one stage at a time to get to the center before time runs out
public class MassEffectLockpickGame : LockpickGame
{

    Player thePlayer;

    //The rotational speed of the player's cursor, in degrees per second
    public float playerSpeed = 60.0f;

    //The speed at which the moving obstacles move
    public float obstacleSpeed = 30f;

    //Keep track of the player's current rotation
    float currentRotation = 0.0f;

    //Because we're moving inwards, the player starts at level 8 and the goal is level 3 (because the center circle is 3 units thick)
    int currentLevel = 8;

    //The static and dynamic obstacles. RectTransforms because we're mostly concerned with positioning/moving them
    public List<RectTransform> StaticObstacles;
    public List<RectTransform> DynamicObstacles;

    //Tunable value for the time in seconds the player has to complete the game
    public float timerStartValue = 12f;
    //Current timer value
    float timerValue;
    //Keep track of the failed state, so we don't respond to input or do other things while we're there
    bool failed = false;

    //The player has a little time before we start penalizing them for collisions, this tracks that
    float collisionTimer = 0.0f;

    //UI
    public GameObject Panel;

    //The player cursor's transform
    public RectTransform playerCursor;

    //The text that displays the time remaining
    public TMPro.TextMeshProUGUI timerText;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupLock();
    }

    void SetupLock()
    {
        //Re-initialize our rotation and level values to their start
        currentRotation = 0.0f;
        currentLevel = 8;
        //Reset the player cursor's rotation and pivot value (distance to the center)
        playerCursor.localRotation = Quaternion.Euler(0, 0, currentRotation);
        playerCursor.pivot = new Vector2(0.5f, currentLevel);
        //Set up each static obstacle somewhere on the circle
        foreach (RectTransform rt in StaticObstacles)
        {
            float RandomDegrees = Random.Range(0f, 359f);
            rt.localRotation = Quaternion.Euler(0, 0, RandomDegrees);
        }

        //The dynamic obstacles will move, but we should set them up with random starting positions, too
        foreach (RectTransform rt in DynamicObstacles)
        {
            float RandomDegrees = Random.Range(0f, 359f);
            rt.localRotation = Quaternion.Euler(0, 0, RandomDegrees);
        }

        timerValue = timerStartValue;
        failed = false;
        //The cursor has its own script, in order to receive collisions, but it sends the collision back to the main game so it needs a reference
        playerCursor.GetComponent<MassEffectCursor>().lockpickGame = this;
        collisionTimer = 0f;
    }

    //This closes the game and puts the player back into the world
    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If we fail, display text so that the player knows, and then leave the game after a delay
    public override void OnFailure()
    {
        timerText.text = "FAILED!";
        failed = true;
        Debug.Log("Failed!");
        StartCoroutine(FailureDelay());
    }

    //Simple coroutine for leaving after a short delay
    IEnumerator FailureDelay()
    {
        yield return new WaitForSeconds(2f);
        EndLockpicking();
    }

    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    //This is called from the cursor script when there's a new collision. If the grace period has passed, the player fails
    public void CursorCollision()
    {
        if (collisionTimer > 2f)
        {
            OnFailure();
        }
    }

    void Update()
    {
        //This tracks the player's "grace period" with respect to collisions, giving them 2 seconds from the start of the game before we fail them for colliding
        if (collisionTimer < 2f)
        {
            collisionTimer += Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        //If we're in the failure state, then don't worry about any other player input or game logic
        if (failed)
        {
            return;
        }
        //If the player presses left or right, we rotate the cursor according to the speed (with the time taken into account in order to be framerate-independent)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentRotation -= playerSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentRotation += playerSpeed * Time.deltaTime;
        }
        //If the player moves into the next ring, we move them forward. We do this by incrementing the level and using that to calculate the pivot (see below)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentLevel > 3)
            {
                currentLevel -= 1;
                //When we move the pivot, we have to also change the offset of the collider to move it up as well. 
                //The offset of the collider is relative to the pivot (50 pixels per level, plus 25 to center it on the sprite)
                playerCursor.GetComponent<Collider2D>().offset = new Vector2(0, (currentLevel * -50f) + 25f);
            }
        }
        //If the player has made it to the center of the ring, they win!
        if (currentLevel <= 3)
        {
            //Success!
            Debug.Log("Success!");
            OnSuccess();
        }
        //Move all the dynamic obstacles at their speed
        foreach (RectTransform rt in DynamicObstacles)
        {
            float newRot = rt.localRotation.eulerAngles.z + (obstacleSpeed * Time.deltaTime);
            rt.localRotation = Quaternion.Euler(0, 0, newRot);
        }

        //So, this is probably not a great idea. But, I'm taking advantage of a weird quirk here in Unity - changing the pivot like this moves the object closer to its pivot. 
        //Because of the relative sizes of the cursor and the rings, a change of 1 in the pivot value moves up one ring level. (essentially, 50 pixels, which is the size of the rings and the cursor)
        //If you want to reproduce this game, you probably shouldn't do this - instead, use an invisible parent object that you rotate instead and move the cursor relative to the parent.
        //That will also avoid the need to move the collider.
        //Why did I do it this way? Hubris
        playerCursor.pivot = new Vector2(0.5f, currentLevel);

        //Update the cursor rotation based on the player input
        playerCursor.localRotation = Quaternion.Euler(0, 0, currentRotation);

        //Count down the timer. If we've reached 0, then the player has failed. Otherwise, display the new timer value (formatted to 2 decimal places)
        timerValue -= Time.deltaTime;
        if (timerValue <= 0f)
        {
            OnFailure();
        }
        else
        {
            timerText.text = timerValue.ToString("F2");
        }
    }

    public override string GetFriendlyName()
    {
        return "Mass Effect";
    }
}
