using UnityEngine;
using System.Collections;
using TMPro;

//This is a pretty simple minigame - it just involves rolling a die and adding numbers to it! All of this is just displayed in text on the UI
//so the code is very simple
public class DNDLockpickGame : LockpickGame
{
    //Game information - the DC (difficulty class), skill level, total result, and roll result
    int CurrentDC;
    int CurrentSkill;
    int CurrentResult;
    int DiceResult;

    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;

    //The text where we display the various numbers in the calculation
    public TextMeshProUGUI DCText;
    public TextMeshProUGUI SkillText;

    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI SuccessText;

    public TextMeshProUGUI DiceText;

    Player ThePlayer;

    public override void BeginLockpicking(Player player)
    {
        //Set us up with some basic values
        GamePanel.SetActive(true);
        CurrentDC = 10;
        CurrentSkill = 2;
        CurrentResult = 0;
        
        UpdateText();
        player.FreezePlayer();
        ThePlayer = player;
    }

    public override void EndLockpicking()
    {
        GamePanel.SetActive(false);
        ThePlayer.UnfreezePlayer();
    }

    public override void OnFailure()
    {
        SuccessText.text = "Failure";
    }

    public override void OnSuccess()
    {
        OpenLock();
        SuccessText.text = "Success!";
    }

    //Update all our text, and also display the result of the player's roll
    public void UpdateText()
    {
        DCText.text = CurrentDC.ToString();
        SkillText.text = CurrentSkill.ToString();

        ResultText.text = CurrentResult.ToString();
        DiceText.text = DiceResult.ToString();

        SuccessText.text = "";

        if (DiceResult == 20 || (CurrentResult > CurrentDC) && DiceResult != 1)
        {
            OnSuccess(); 
        }
        else
        {
            OnFailure();
        }
    }

    //Roll some dice - called by one of the UI buttons
    public void Roll()
    {
        //Random.Range(int, int) excludes the higher end so we have to go 1-21 to get a result between 1 and 20
        DiceResult = Random.Range(1, 21);
        //Add the die roll to the player's skill
        CurrentResult = DiceResult + CurrentSkill;
        DiceText.text = DiceResult.ToString();
        UpdateText();
    }

    //Increment the player's skill (called by one of the buttons)
    public void IncrementSkill()
    {
        
        CurrentSkill += 1;
        if (CurrentSkill > 20)
        {
            CurrentSkill = 20;
        }
        UpdateText();
    }

    //Same for decrementing
    public void DecrementSkill()
    {
        CurrentSkill -= 1;
        if (CurrentSkill < -5)
        {
            CurrentSkill = -5;
        }
        UpdateText();
    }

    //And same for the DC
    public void IncrementDC()
    {
        CurrentDC++;
        if (CurrentDC > 30)
        {
            CurrentDC = 30;
        }
        UpdateText();
    }

    public void DecrementDC()
    {
        CurrentDC--;
        if (CurrentDC < 5)
        {
            CurrentDC = 5;
        }
        UpdateText();
    }

    public override string GetFriendlyName()
    {
        return "Dungeons and Dragons: 5E";
    }
}
