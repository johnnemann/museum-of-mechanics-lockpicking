using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//In Pathologic, the player attempts to raise two "tusk" objects to align both in the right place at the right time. If they're not being actively raised, the tusks fall
//Certain areas of the tusks are "success" zones that the player must align with the indicators, and others are "slow" zones where the movement rate of the tusks changes
//This game plays best with a controller, using the right and left triggers, but for this version only the mouse controls currently function
public class Pathologic2LockpickGame : LockpickGame
{

    Player thePlayer;

    //Tunable values for the movement speed of the tusks, including when they are moving slowly (in slow zones), or falling
    public float movementSpeed = 0.4f;
    public float slowMovementSpeed = 0.2f;
    public float fallSpeed = 0.3f;

    //Base positions for resetting the tusk elements, and max positions for limiting their movement
    public float leftYPos = 69f;
    public float leftYMax = 250f;

    public float rightYPos = 79f;
    public float rightYMax = 250f;

    //indexes tracking the slow zones, success zones, and current zone
    int leftSlowZone = 0;
    int rightSlowZone = 0;

    int leftSuccessZone = 0;
    int rightSuccessZone = 0;

    int currentLeftZone = -1;
    int currentRightZone = -1;

    //We need to know independently whether the right or left tusk is in a success zone, and when both of them are true that they've been there for long enough
    bool successWait = false;
    bool rightSuccess = false;
    bool leftSuccess = false;

    //This is how long the player has to hold both tusks in the success zone before the game ends
    public float successWaitTime = 3.0f;
    float successWaitTimer = 0.0f;

    //UI
    public GameObject Panel;

    //Transforms for the tusks, since we are mostly concerned with moving them around
    public Transform leftTusk;
    public Transform rightTusk;

    //The needles object that indicates the position of the tusks, and the zone objects
    public GameObject needles;
    public GameObject[] leftZones;
    public GameObject[] rightZones;

    //Colors for the two special types of zone - "neutral" zones are default color
    public Color slowZoneColor = Color.black;
    public Color successZoneColor = Color.red;

    public override void BeginLockpicking(Player player)
    {
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();

        SetupLock();
    }

    //This function initializes the minigame when we start playing or reset
    void SetupLock()
    {
        //To start, clear the slow zones
        rightSlowZone = leftSlowZone = -1;
        
        //Reset zone colors
        foreach (GameObject gc in leftZones)
        {
            gc.GetComponent<Image>().color = Color.white;
        }
        //Generate "success" and "slow" zones
        //Don't choose the first or last zone as the success zone
        leftSuccessZone = Random.Range(1, leftZones.Length - 1);
        leftZones[leftSuccessZone].GetComponent<Image>().color = successZoneColor;
        if (Random.value < 0.75f)
        {
            leftSlowZone = leftSuccessZone;
            while (leftSlowZone == leftSuccessZone)
            {
                leftSlowZone = Random.Range(1, leftZones.Length - 1);
            }
            leftZones[leftSlowZone].GetComponent<Image>().color = slowZoneColor;
        }

        rightSuccessZone = Random.Range(1, rightZones.Length - 1);
        rightZones[rightSuccessZone].GetComponent<Image>().color = successZoneColor;
        if (Random.value < 0.75f)
        {
            rightSlowZone = rightSuccessZone;
            while (rightSlowZone == rightSuccessZone)
            {
                rightSlowZone = Random.Range(1, rightZones.Length - 1);
            }
            rightZones[rightSlowZone].GetComponent<Image>().color = slowZoneColor;
        }
        successWaitTimer = 0.0f;
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();

    }

    //No failure case for this game - in the real game it breaks lockpicks but that doesn't matter here
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

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

        //Figure out whether the player is raising the left tusk or not. If so, move them up using their movement speed
        float leftMovement = 0.0f;
        float rightMovement = 0.0f;
        if (Input.GetMouseButton(0))
        {
            //If we're in a slow zone, use the slow movement speed
            if (currentLeftZone == leftSlowZone)
            {
                leftMovement = slowMovementSpeed * Time.deltaTime;
            }
            //Otherwise just normal speed
            else
            {
                leftMovement = movementSpeed * Time.deltaTime;
            }
        }
        //If not, they fall
        else
        {
            leftMovement = -fallSpeed * Time.deltaTime;
        }
        //Smae thing for the right tusk
        if (Input.GetMouseButton(1))
        {
            if (currentRightZone == rightSlowZone)
            {
                rightMovement = slowMovementSpeed * Time.deltaTime;
            }
            else
            {
                rightMovement = movementSpeed * Time.deltaTime;
            }
            
        }
        else
        {
            rightMovement = -fallSpeed * Time.deltaTime;
        }

        //Then, move the actual tusk object depending on the accumulated movement. Check it against the bounds, too
        leftTusk.localPosition += new Vector3(0f, leftMovement, 0f);
        //If we've gone too high or too low, just set our position to the max or min
        if (leftTusk.localPosition.y > leftYMax)
        {
            leftTusk.localPosition = new Vector3(leftTusk.localPosition.x, leftYMax, 0f);
        }
        if (leftTusk.localPosition.y < leftYPos)
        {
            leftTusk.localPosition = new Vector3(leftTusk.localPosition.x, leftYPos, 0f);
        }
        //And same on the right
        rightTusk.localPosition += new Vector3(0f, rightMovement, 0f);
        if (rightTusk.localPosition.y > rightYMax)
        {
            rightTusk.localPosition = new Vector3(rightTusk.localPosition.x, rightYMax, 0f);
        }
        if (rightTusk.localPosition.y < rightYPos)
        {
            rightTusk.localPosition = new Vector3(rightTusk.localPosition.x, rightYPos, 0f);
        }

        //Now we test to see how we're doing, if we've hit the success zone or not, and also assign our current zone
        //Instead of using IsTouching() we could have done a trigger callback like in the Mass Effect game but this is fine since there are so few zones to test
        for(int i =0;i<leftZones.Length;i++)
        {
            if (needles.GetComponent<BoxCollider2D>().IsTouching(leftZones[i].GetComponent<BoxCollider2D>()))
            {
                currentLeftZone = i;
                if (i == leftSuccessZone)
                {
                    //If we're in the zone, mark our bool as true
                    leftSuccess = true;
                }
                //Otherwise, clear the success wait flag
                else
                {
                    successWait = false;
                }
                break;
            }
        }
        //Same for the right
        for (int i = 0; i < rightZones.Length; i++)
        {
            if (needles.GetComponent<BoxCollider2D>().IsTouching(rightZones[i].GetComponent<BoxCollider2D>()))
            {
                currentRightZone = i;
                if (i == rightSuccessZone)
                {
                    rightSuccess = true;
                }
                else
                {
                    successWait = false;
                }
                break;
            }
        }

        //If the needles are in both success zones, then we set the wait flag
        if (rightSuccess && leftSuccess)
        {
            successWait = true;
        }

        //If the wait flag is set, then we run the timer
        if (successWait)
        {
            successWaitTimer += Time.deltaTime;
            //And if the time gets to be enough, the player wins
            if (successWaitTimer >= successWaitTime)
            {
                OnSuccess();
            }
        }
        //Otherwise, we reset the timer to zero
        else
        {
            successWaitTimer = 0.0f;
        }
    }

    public override string GetFriendlyName()
    {
        return "Pathologic 2";
    }
}
