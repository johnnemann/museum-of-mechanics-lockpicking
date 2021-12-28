using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//A note:
//The non-open-source version of the Museum uses a Unity Asset Store package called "InControl" to handle controller and MKB input
//It's a good package, I think it's worth a purchase. Obviously, though, I cannot distribute that code as part of this
//release. So the input code here has been modified to use only Unity, and as a consequence may only support MKB controls, depending on game
//This also means input handling may be slightly worse than the other version of the game


//In Thief 1 and 2, there's not really a minigame. The player just applies the lockpick to the lock for a set amount of time. Sometimes, the pick stops working
//and the player has to change picks
public class Thief12LockpickGame : LockpickGame
{
    Player myPlayer;

    public GameObject Panel;

    //Gameobjects for the two lockpick models
    public GameObject Lockpick1;
    public GameObject Lockpick2;

    //The currently-active one
    GameObject ActiveLockpick;

    bool LockpickingMode = false;
    bool LockpickingExtending = false;

    //Where the lockpicks rest when not being used vs. being applied
    public Transform LockpickNeutralTransform;
    public Transform LockpickActiveTransform;

    //Track the percentage progress
    float LockpickProgress;

    //Tunable length and number of stages
    public float LockpickStageLength = 1.0f;
    public int NumberOfStages = 2;

    //The progress bar image that uses filled sprites to track progress
    public Image ProgressBar;
    public TextMeshProUGUI ProgressText;

    //In the regular game, we use models attached to the player in 3d space and the museum lock to simulate the Thief lockpicking.
    //Since here we don't have the 3d space, we use some models projected onto the UI with a render texture from a special camera
    public Camera thiefCamera;


    public override void BeginLockpicking(Player player)
    {
        player.FreezePlayer();
        myPlayer = player;
        EnterLockpickingMode();
        LockpickProgress = 0.0f;
        Panel.SetActive(true);
        thiefCamera.gameObject.SetActive(true);
    }

    public override void EndLockpicking()
    {
        myPlayer.UnfreezePlayer();
        Panel.SetActive(false);
        ExitLockpickingMode();
        thiefCamera.gameObject.SetActive(false);
    }

    //No failure in this game
    public override void OnFailure()
    {
        throw new System.NotImplementedException();
    }

    //If we succeed, open the lock and close the game
    public override void OnSuccess()
    {
        OpenLock();
        EndLockpicking();
    }

    public void EnterLockpickingMode()
    {
        LockpickingMode = true;
        ActiveLockpick = Lockpick1;
        Lockpick1.SetActive(true);
        Lockpick1.transform.SetPositionAndRotation(LockpickNeutralTransform.position, LockpickNeutralTransform.rotation);
    }

    public void ExitLockpickingMode()
    {
        LockpickingMode = false;
        Lockpick1.SetActive(false);
        Lockpick2.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EndLockpicking();
        }

        if (LockpickingMode)
        {
            //In lockpicking mode, the player can extend the lockpick to make progress
            if (Input.GetMouseButton(0))
            {
                //Extend lockpick
                if (!LockpickingExtending)
                {
                    //If we're extending, then tween towards the finished position
                    DOTween.CompleteAll();
                    LockpickingExtending = true;
                    ActiveLockpick.transform.DOMove(LockpickActiveTransform.position, 1.0f).SetEase(Ease.InOutCubic);
                }
                //If we're already fully extended, then just make progress
                else
                {
                    if (LockpickProgress >= 1.0f)
                    {
                        //Success!
                        OnSuccess();
                    }
                    //This is the logic for making the player switch lockpicks. Since there's only two lockpicks, they just have to switch once, at the halfway point
                    //If they're using the right lockpick, just keep progressing
                    if ((LockpickProgress < 0.5f && ActiveLockpick == Lockpick1) || (LockpickProgress >= 0.5 && ActiveLockpick == Lockpick2))
                    {
                        LockpickProgress += (((NumberOfStages * LockpickStageLength / 100.0f)) * Time.deltaTime * 10.0f);
                        ProgressBar.fillAmount = LockpickProgress;
                        ProgressText.text = (LockpickProgress * 100.0f).ToString("F1");
                    }
                    else
                    {
                        //Not making progress, should play clicking sound or something
                    }
                }
            }
            else
            {
                //Retract lockpick
                if (LockpickingExtending)
                {
                    DOTween.CompleteAll();
                    LockpickingExtending = false;
                    ActiveLockpick.transform.DOMove(LockpickNeutralTransform.position, 1.0f).SetEase(Ease.InOutCubic);
                }
            }
            //If the player switches lockpicks, just swap the gameobject that's active
            if (Input.GetMouseButton(1))
            {
                if (LockpickingMode)
                {
                    if (ActiveLockpick == Lockpick1)
                    {
                        ActiveLockpick = Lockpick2;
                        Lockpick1.SetActive(false);
                        Lockpick2.SetActive(true);
                    }
                    else
                    {
                        ActiveLockpick = Lockpick1;
                        Lockpick2.SetActive(false);
                        Lockpick1.SetActive(true);
                    }
                }
            }
        }
    }

    public override string GetFriendlyName()
    {
        return "Thief 1 + 2";
    }
}
