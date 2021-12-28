using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game

//In The Testament of Sherlock Holmes, the player bends a lockpick to match the prompt. The lockpick has a number of segments that can be bent at a few angles, and
//angles accumulate along (so if you bend the first segment 30 degrees and the second 30, the total bend of the third is now 60 degrees)
public class TestamentLockpickGame : LockpickGame
{
    Player thePlayer;

    //UI Stuff
    public GameObject Panel;

    //Images for the lockpick segments
    public Image[] Segments;

    //The prompt diagram that the player is attempting to match
    public RectTransform[] DiagramSegments;

    //The tumblers - these are mostly decorative
    public RectTransform[] Tumblers;

    //The pins, also decorative
    public Image[] Pins;

    //Which segment the player is currently manipulating
    int SelectedSegment;

    //The levels track the numerical amount of bend in each segment
    int[] SegmentLevels;
    int[] DiagramLevels;

    public override void BeginLockpicking(Player player)
    {
        SegmentLevels = new int[Segments.Length];
        DiagramLevels = new int[Segments.Length];
        Panel.SetActive(true);
        thePlayer = player;
        thePlayer.FreezePlayer();
        Reset();
        SelectSegment(3);
        GenerateGoal();
    }


    private void Reset()
    {
        //Go through the segments of both the pick and the diagram and make sure they're all rotated back to 0
        for (int i = 0; i < Segments.Length; i++)
        {
            Segments[i].GetComponent<RectTransform>().rotation = Quaternion.identity;
            DiagramSegments[i].GetComponent<RectTransform>().rotation = Quaternion.identity;
            
        }
        //Go through the tumblers and set them to neutral
        for (int i = 0; i < Tumblers.Length; i++)
        {
            Vector3 pos = Tumblers[i].anchoredPosition;
            pos.y = 57.5f;
            Tumblers[i].anchoredPosition = pos;
        }
        //And the pins
        for (int i = 0; i < Pins.Length; i++)
        {
            Pins[i].fillAmount = 1.0f;
        }

        for (int i = 0; i < SegmentLevels.Length; i++)
        {
            SegmentLevels[i] = 0;
            DiagramLevels[i] = 0;
        }
    }

    public override void EndLockpicking()
    {
        Panel.SetActive(false);
        thePlayer.UnfreezePlayer();
    }

    //If we fail, nothing happens except the lock doesn't open
    public override void OnFailure()
    {
        Debug.Log("Failed!");
    }

    //If the player succeeds, open the in-world lock and close the minigame
    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            AdjustUp();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            AdjustDown();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SelectSegment(SelectedSegment - 1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SelectSegment(SelectedSegment + 1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckLockpick();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }
    }

    //Called when the player moves the current segment up
    void AdjustUp()
    {
        //If we're already at the max adjustment, ignore this
        if (SegmentLevels[SelectedSegment] >= 2)
        {
            return;
        }
        else
        {
            //Starting at this segment, iterate through the segments and adjust each subsequent one by the same amount
            for (int i = SelectedSegment; i < SegmentLevels.Length; i++)
            {
                SegmentLevels[i] += 1; 
            }
            //Rotate this segment to 30 degrees * the current level
            Segments[SelectedSegment].GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(SegmentLevels[SelectedSegment] * 30f, Vector3.forward);
        }
    }

    //Same thing, but downwards
    void AdjustDown()
    {
        if (SegmentLevels[SelectedSegment] <= -2)
        {
            return;
        }
        else
        {
            for (int i = SelectedSegment; i < SegmentLevels.Length; i++)
            {
                SegmentLevels[i] -= 1;
            }
            Segments[SelectedSegment].GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(SegmentLevels[SelectedSegment] * 30f, Vector3.forward);
        }
    }

    //Check the lockpick against the diagram
    void CheckLockpick()
    {
        //Move along segments, comparing each to the diagram angle. If all match, succeed. Otherwise, failure
        bool failed = false;
        for (int i = 0; i < SegmentLevels.Length; i++)
        {
            if (SegmentLevels[i] != DiagramLevels[i])
            {
                failed = true;
                break;
            }
        }
        if (!failed)
        {
            OnSuccess();
        }
        else
        {
            OnFailure();
        }
    }

    //This visually selects the segment to let the player know what they're bending
    void SelectSegment(int seg)
    {
        seg = Mathf.Clamp(seg, 1, Segments.Length-1);
        //Highlight this segment and everything to the right of it
        SelectedSegment = seg;
        for (int i = 0; i < Segments.Length; i++)
        {
            if (i >= seg)
            {
                Segments[i].color = Color.cyan;
            }
            else
            {
                Segments[i].color = Color.white;
            }
        }
    }

    //This is a function to generate a valid puzzle. In the actual game, there's just one, authored puzzle but I thought it would be fun to write a generator
    void GenerateGoal()
    {
        //Keep track of the total angle - we don't want to loop back, so if the total is ever greater than 90 or less than -90 we need to rethink
        int totalAngle = 0;
        //Start at 1 - we don't move the first segment
        for (int i = 1; i < DiagramSegments.Length; i++)
        {
            //Generate an angle between -60 and 60 degrees
            int level = Random.Range(-2, 3);
            //Failsafe is here to make sure this loop is never infinite, in case we somehow hit a really bad string of random numbers
            //This isn't really necessary, but without it this loop could *theoretically* be infinite
            int failsafe = 0;
            while ((totalAngle + level > 2 || totalAngle + level < -2) && failsafe < 1000)
            {
                level = Random.Range(-2, 3);
                failsafe++;
            }

            DiagramSegments[i].GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(level * 30f, Vector3.forward);
            DiagramLevels[i] = level;
            totalAngle += level;
        }

        AdjustPinsAndTumblers();
    }

    //This is just a cosmetic effect. The game shows a path through the pins and tumblers of the lock that fits the bent pin. We just adjust the pins appropriately 
    void AdjustPinsAndTumblers()
    {
        Vector2 pos = new Vector2();
        int SegmentRotation = 0;
        for (int i = 0; i < DiagramLevels.Length; i++)
        {
            
            //Each segment has two pins
            SegmentRotation += DiagramLevels[i] * 30;

            switch(SegmentRotation)
            {
                case 0:
                    //At 0 (flat), both pins are the same value
                    Pins[i*2].fillAmount = 0.6f;
                    Pins[i*2 + 1].fillAmount = 0.6f;
                    break;
                case 30:
                    //At 30, we're tilting upwards
                    Pins[i * 2].fillAmount = 0.6f;
                    Pins[i * 2 + 1].fillAmount = 0.7f;
                    pos = Tumblers[i + 1].anchoredPosition;
                    pos.y += 30;
                    Tumblers[i + 1].anchoredPosition = pos;
                    break;
                case 60:
                    //At 60, we're almost vertical!
                    Pins[i * 2].fillAmount = 0.85f;
                    Pins[i * 2 + 1].fillAmount = 1f;
                    pos = Tumblers[i + 1].anchoredPosition;
                    pos.y += 40;
                    Tumblers[i + 1].anchoredPosition = pos;
                    break;
                case -60:
                    Pins[i * 2].fillAmount = 0.15f;
                    Pins[i * 2 + 1].fillAmount = 0.15f;
                    pos = Tumblers[i + 1].anchoredPosition;
                    pos.y -= 50;
                    Tumblers[i + 1].anchoredPosition = pos;
                    break;
                case -30:
                    Pins[i * 2].fillAmount = 0.4f;
                    Pins[i * 2 + 1].fillAmount = 0.25f;
                    pos = Tumblers[i + 1].anchoredPosition;
                    pos.y -= 40;
                    Tumblers[i + 1].anchoredPosition = pos;
                    break;
            }
            
        }
    }

    public override string GetFriendlyName()
    {
        return "The Testament of Sherlock Holmes";
    }
}
