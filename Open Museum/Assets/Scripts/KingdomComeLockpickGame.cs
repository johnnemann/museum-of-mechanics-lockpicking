using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//Kingdom Come's lockpicking game asks the player to find a hidden hotspot with the cursor, and then to keep the cursor on that hotspot while they rotate the lock
//fully (thus moving the hotspot)
public class KingdomComeLockpickGame : LockpickGame
{

    Player thePlayer;

    //The position the player is aiming for
    Vector3 targetPosition;

    //The distance within which we will be considered "close" to the target..At this point we change the color and size to let the player know
    public float closeDistance;

    //If we're currently in the failure state
    bool failed;

    //The player has some time to recover if they move out of the zone, this tracks that
    float timeFailing = 0.0f;

    //The current rotation of the whole lock
    float currentRotation = 0.0f;

    //Tunable values for the speed of rotation and the time the player has outside the zone before they fail
    public float rotationSpeed = 0.1f;

    public float timeToFailure = 1.0f;

    //UI
    public GameObject Panel;

    //The player-controlled cursor. We keep a transform because we're mostly concerned with moving it
    public Transform cursorTransform;

    //The image that indicates the position of the cursor. We have a reference to this component to grow/shrink it and change its color
    public Image cursorIndicator;

    //The target of the minigame
    public RectTransform targetObject;

    //Keep track of our starting color and the color for being close to the target so that we can lerp between them
    public Color startingCursorColor;
    public Color cursorSuccessColor;

    //Again, the size of the cursor when we're in the middle of the target, as a lerp goal
    public Vector3 cursorSuccessSize;

    //The inside ring on the lock rotates as we make progress
    public Transform rotatingRing;

    //The text to display when the player fails
    public TMPro.TextMeshProUGUI failureText;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();


        SetupLock();
    }

    //Sets up the initial state of the lock, when we begin a game
    public void SetupLock()
    {
        //generate the correct position, outside of 75 units away (the inner circle) but within 235 units away (the outer circle). 
        //Obviously this should ideally pull from the actual radius of the images used, either set in the editor or calculated from the image
        //But, this is quick and dirty and will work for our case
        targetPosition = Random.insideUnitCircle.normalized;

        float distance = Random.Range(75f, 235f);
        //We take the random distance we've generated, and a random direction on a circle, and combine them to get a random point on the donut that's the player's target
        targetPosition = targetPosition * distance;
        //Move the target object there, so we can use its collider to test against
        targetObject.localPosition = targetPosition;
        //reset cursor color and size
        cursorIndicator.color = startingCursorColor;
        cursorIndicator.transform.localScale = Vector3.one;
        //Reset our failure state
        timeFailing = 0.0f;
        failed = false;
        //reset the rotations of the lock
        currentRotation = 0;
        rotatingRing.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    //Finishes the game and puts the player back out into the museum
    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //Called if the player fails by staying outside the zone for too long. In this case we put ourselves into a new state where the puzzle stops moving,
    //and set the text active to let the player know they've failed.
    public override void OnFailure()
    {
        failed = true;
        failureText.gameObject.SetActive(true);
        //Start the coroutine to hide the failure text after some time
        StartCoroutine(HideFailureText());
    }

    //If we succeed we open the in-world lock and leave the game
    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
        EndLockpicking();
    }

    //Coroutine to wait three seconds and then hide the failure text, then reset the game
    IEnumerator HideFailureText()
    {
        yield return new WaitForSeconds(3);
        failureText.gameObject.SetActive(false);
        SetupLock();
    }

    //Handle player input and game logic
    void Update()
    {
        //The player can always quit, so check this before the failure state
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
        //If we're in the failed state, then just leave - the player can't do anything else at this point until we're reset
        if (failed)
        {
            return;
        }
        //Show hints - just reveal the (usually invisible) image attached to the target object
        if (Input.GetKeyDown(KeyCode.T))
        {
            targetObject.GetComponent<Image>().enabled = !targetObject.GetComponent<Image>().enabled;
        }
        //Get our screensize to figure out where the mouse cursor is
        Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0);
        //Get mouse or stick position, move cursor to position within the bounds of the ring (235 units in any direction)
        Vector3 cursorPosition;

        //For analog stick. This is not very good/precise input but it gets the idea across
        //This code is commented out because it depends on InControl to make the assessment if whether the player is using a controller or not
        //But I have left it in place so that you can see the (very trivial) math that goes into this
        /*
        if (playerActions.LastInputType == BindingSourceType.DeviceBindingSource)
        {
            //If the player is using a controller, take the left stick's current value and normalize it to our circle.
            //Ideally this would have smoothing code and so on, but right now we're just using the raw value
            cursorPosition = InputManager.ActiveDevice.LeftStick.Value * 235f;
        }
        */
        //For mouse
        //else
        {
            //The mouse position is in different coordinates than our UI, so we have to convert
            cursorPosition = Input.mousePosition - screenSize / 2;
        }

        //Get the distance the cursor position is from the origin
        float distance = Vector3.Distance(cursorPosition, Vector3.zero);

        //Keep the cursor locked within the lock's circle by limiting its position to within those bounds
        //TODO: Magic number
        if (distance > 235f)
        {
            //Calculate the place to put the cursor on the edge if we would otherwise be past it
            //This math will work no matter the origin point. Since it's zero, it could be simplified but I'm leaving this in so it's clear what needs to happen if this moves
            Vector3 fromOriginToObject = cursorPosition - Vector3.zero;
            fromOriginToObject *= 235f / distance;
            cursorPosition = Vector3.zero + fromOriginToObject;
        }
        //Set the cursor object's position to the new position
        cursorTransform.localPosition = cursorPosition;
        
        //Figure out if we're within the close distance by doing math to see how close the player's cursor is to the target object
        //Even though we have the colliders on the objects, it's easier to do this just with math, since we want to know how close, not just whether they're touching.
        float distanceToTarget = Vector3.Distance(cursorTransform.position, targetObject.position);

        //If we are within the close distance, then lerp the color and size of the cursor to indicate that to the player
        if (distanceToTarget < closeDistance)
        {
            //if cursor is close to target, lerp towards success color and size
            cursorIndicator.color = Color.Lerp(startingCursorColor, cursorSuccessColor, 1-(distanceToTarget/closeDistance));
            cursorIndicator.rectTransform.localScale = Vector3.Lerp(Vector3.one, cursorSuccessSize, 1 - (distanceToTarget / closeDistance));

        }
        //Handle the player rotating the lock
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Check to see whether the cursor is touching the target. If not, keep track of the amount of time we're failing and don't move the lock
            if (!cursorTransform.GetComponent<CircleCollider2D>().IsTouching(targetObject.GetComponent<BoxCollider2D>()))
            {
                //maybe we jiggle? We should keep track of how long we're here, though, and fail out if we go over a certain amount
                timeFailing += Time.deltaTime;
                //If the player's cursor has been outside the zone too long, then they fail
                if (timeFailing > timeToFailure)
                {
                    //Failure - show text about "lockpick broke!" and reset lock
                    OnFailure();
                }
            }
            //Otherwise, we are touching and everything's good, we can move the lock
            else
            {
                //Rotate, taking time into account so that we're framerate-independent
                currentRotation -= rotationSpeed * Time.deltaTime;

                //If we've rotated far enough, then the player wins
                if (currentRotation <= -250)
                {
                    //Success!
                    OnSuccess();
                }
                //Move the actual lock. Because the target is a child of the ring, it will rotate with the lock, but the cursor will not (providing the challenge!)
                rotatingRing.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
        //If the player is not holding down the rotate button, then we move the ring back the other way (unless it's already at the beginning)
        else
        {
            if (currentRotation < 0)
            {
                currentRotation += rotationSpeed * Time.deltaTime;
                rotatingRing.localRotation = Quaternion.Euler(0, 0, currentRotation);
            }
        }
    }

    public override string GetFriendlyName()
    {
        return "Kingdom Come: Deliverance";
    }
}
