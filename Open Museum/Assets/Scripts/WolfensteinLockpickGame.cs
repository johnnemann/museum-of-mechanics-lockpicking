using DG.Tweening;
using UnityEngine;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In Wolfenstein, the player attempts to keep a pointer aligned with a mark on the lock
//If they do that for long enough, the lock opens
public class WolfensteinLockpickGame : LockpickGame
{

    Player thePlayer;

    //Speed of the player's cursor in degrees per second
    public float playerRotationSpeed = 10.0f;

    //The amount of time the lock stays at one position
    //TODO: Make this a range of times and generate it anew each time
    public float lockRotationTime = 1.0f;

    float lockRotationTimer = 0f;

    //The speed at which the lock itself rotates, in degrees per second
    public float lockRotationSpeed = 60f;

    //Current rotation, in degrees, of the inner (player-controlled) circle
    float innerRotation;

    //Rotation of the outer "target" circle
    float outerRotation;

    //The amount of time the player must keep the pointer aligned until a tumbler pops up.
    public float timeToSuccess;

    float successTimer;

    //The amount of time until a locked tumbler retracts
    public float tumblerRetractTime;

    float retractTimer;

    //The tumbler on which the player is working right now
    int currentTumbler = 0;

    //UI
    public GameObject Panel;

    public Transform outerRing;

    public Transform innerRing;

    public GameObject[] tumblers;

    public Collider2D pointerCollider;

    public Collider2D targetCollider;

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

    //There's no real failure case in this game, at worst we lose progress
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }
    
    //On success, open the in-world lock and end the game
    public override void OnSuccess()
    {
        Debug.Log("Success!");
        OpenLock();
        EndLockpicking();
    }

    //Called to set up the initial values for the game
    void SetupLock()
    {
        //Set our timers to 0
        successTimer = 0f;
        retractTimer = 0f;

        //Start at the first tumbler
        currentTumbler = 0;
        //orient the lock at neutral
        outerRing.rotation = Quaternion.Euler(Vector3.zero);
        innerRing.rotation = Quaternion.Euler(Vector3.zero);

        innerRotation = 0f;
        //hide the tumblers
        foreach (GameObject go in tumblers)
        {
            go.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        //If the player rotates left or right, then add that movement to the total (checking for bounds)
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            innerRotation -= playerRotationSpeed * Time.deltaTime;
            if (innerRotation < -35f)
            {
                innerRotation = -35f;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            innerRotation += playerRotationSpeed * Time.deltaTime;
            if (innerRotation > 35f)
            {
                innerRotation = 35f;
            }
        }

        //Move the actual ring object
        innerRing.localRotation = Quaternion.Euler(0, 0, innerRotation);

        //If we're still working...
        if (currentTumbler < tumblers.Length)
        {
            //rotate outer lock within the wedge, staying at each position for a short time, and moving smoothly between them
            lockRotationTimer -= Time.deltaTime;

            if (lockRotationTimer <= 0f)
            {
                //A random position to rotate to
                outerRotation = Random.Range(-25f, 25f);
                lockRotationTimer = lockRotationTime;

                //Figure out the distance that this rotation moves us, so that we can rotate there smoothly over time
                float rotationDistance = Mathf.Abs(Mathf.DeltaAngle(outerRing.rotation.eulerAngles.z, outerRotation));

                //This is how long it will take to get to that point at our speed
                float rotationTime = rotationDistance / lockRotationSpeed;

                //Use DOTween to smoothly move to that position over that amount of time
                outerRing.DORotate(new Vector3(0, 0, outerRotation), rotationTime);
            }

        }

        //Test pointer collider against target collider and increment time if they overlap
        if (pointerCollider.IsTouching(targetCollider))
        {
            successTimer += Time.deltaTime;
            //If we've been in the zone long enough, then show the current tumbler and move on to the next. If we've got all three, then we win!
            if (successTimer >= timeToSuccess)
            {
                if (currentTumbler < tumblers.Length)
                {
                    tumblers[currentTumbler].SetActive(true);
                    currentTumbler += 1;
                    successTimer = 0f;
                    retractTimer = 0f;
                }
                else
                {
                    //We're all done!
                    OnSuccess();
                }
            }
        }
        else
        {
            //If we're outside the range, then count up until we retract the current tumbler (losing progress)
            retractTimer += Time.deltaTime;
            if (retractTimer >= tumblerRetractTime)
            {
                tumblers[currentTumbler].SetActive(false);
                if (currentTumbler > 0)
                {
                    currentTumbler -= 1;
                }
                successTimer = 0f;
                retractTimer = 0f;
            }
        }
    }

    public override string GetFriendlyName()
    {
        return "Wolfenstein: The New Order";
    }
}
