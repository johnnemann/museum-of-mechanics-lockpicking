using TMPro;
using UnityEngine;
using UnityEngine.UI;


//Fallout 1 and 2 are similar to DnD in that they just use numbers and random chance, the calculations are just slightly different
//So again this is mostly text display
public class Fallout12LockpickGame : LockpickGame
{
    //Track our current stat numbers, as well as the total and the die roll
    int CurrentAgility;
    int CurrentPerception;
    int CurrentResult;
    int DiceResult;

    /*** UI Objects **/
    //The UI panel that the game UI lives on
    public GameObject GamePanel;

    //Text objects for displaying the various values to the player
    public TextMeshProUGUI AgilityText;
    public TextMeshProUGUI PerceptionText;

    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI SuccessText;

    public TextMeshProUGUI DiceText;

    //This is a toggle for whether the player has lockpicking perk or not - if so, they get a flat bonus of 20 to their roll
    public Toggle LockpickToggle;

    Player ThePlayer;

    bool HasRolled = false;

    public override void BeginLockpicking(Player player)
    {
        //Activate the panel and set up our initial values
        GamePanel.SetActive(true);
        CurrentAgility = 6;
        CurrentPerception = 4;
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

    //This function just updates the varies textfields with their results
    public void UpdateText()
    {
        AgilityText.text = CurrentAgility.ToString();
        PerceptionText.text = CurrentPerception.ToString();

        ResultText.text = CurrentResult.ToString();
        DiceText.text = DiceResult.ToString();

        SuccessText.text = "";

        if (HasRolled)
        {

            if (CurrentResult >= DiceResult)
            {
                OnSuccess();
            }
            else
            {
                OnFailure();
            }
        }
    }

    //Called when the player presses the button in the UI, this generates our random number and does the math to see if the player succeeds or not
    public void Roll()
    {
        HasRolled = true;
        DiceResult = Random.Range(0, 100);
        //I got this formula from a Fallout wiki
        CurrentResult = 20 + ((CurrentAgility + CurrentPerception))/2 + (LockpickToggle.isOn?20:0);
        DiceText.text = DiceResult.ToString();
        UpdateText();
    }

    //These functions are called from the UI buttons and just update our internal statuses, keeping the numbers within some reasonable bounds
    public void IncrementPerception()
    {

        CurrentPerception += 1;
        if (CurrentPerception > 10)
        {
            CurrentPerception = 10;
        }
        UpdateText();
    }

    public void DecrementPerception()
    {
        CurrentPerception -= 1;
        if (CurrentPerception < 1)
        {
            CurrentPerception = 1;
        }
        UpdateText();
    }

    public void IncrementAgility()
    {
        CurrentAgility++;
        if (CurrentAgility > 10)
        {
            CurrentAgility = 10;
        }
        UpdateText();
    }

    public void DecrementAgility()
    {
        CurrentAgility--;
        if (CurrentAgility < 1)
        {
            CurrentAgility = 1;
        }
        UpdateText();
    }

    public override string GetFriendlyName()
    {
        return "Fallout 1+2";
    }
}
