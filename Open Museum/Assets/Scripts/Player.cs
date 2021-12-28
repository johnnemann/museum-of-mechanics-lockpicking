using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    Transform PlayerTransform;

    public GameObject MainMenuPanel;

    public List<LockpickGame> lockpickGames;

    public AlphaProtocolLockpickGame AlphaProtocolLockpickGame;

    public RectTransform gameListContentPanel;

    public GameObject gameButtonPrefab;

    //Unneeded in this version, left in for completeness
    /*

    public float InteractionSearchDistance = 3.0f;

    Frobbable CurrentlySelectedFrobbable;

    public bool IsInteracting = false;

    public Camera myCamera;

    public GameObject MainMenuPanel;
      
     */

    void Start()
    {
        PopulateGameMenu();
    }

    //This is commented out because it's not needed in this version, but left here so you can see what the loop looks like for the
    //3D space
    void Update()
    {
        /*
        //Cast a ray forward from the center of the player camera
        RaycastHit hit;
        if (Physics.Raycast(PlayerTransform.position + (PlayerTransform.forward *0.3f), PlayerTransform.forward, out hit))
        {
            //If it's "close" to us, then check to see if it has a Frobbable component
            if (hit.distance <= InteractionSearchDistance)
            {
                Frobbable f = hit.collider.GetComponentInChildren<Frobbable>();

                if (f != null)
                {
                    //If we have a frobbable and it's not the one we've already got, then highlight it
                    if (f != CurrentlySelectedFrobbable)
                    {
                        HighlightObject(f);
                    }
                }
                //If there isn't one, then stop highlighting
                else
                {
                    StopHighlight();
                }
            }
            //If the ray found nothing close, stop highlighting
            else
            {
                StopHighlight();
            }
        }
        //If the ray found nothing at all, stop highlighting
        else
        {
            StopHighlight();
        }
        //If we're interacting, check to make sure we have a frobbable and if so, interact with it
        if (playerActions.Interact.WasPressed)
        {
            if (CurrentlySelectedFrobbable != null && !IsInteracting)
            {
                CurrentlySelectedFrobbable.OnFrob(this);
            }
            
        }
        //Open the menu if we're not currently interacting with something
        if (playerActions.Menu.WasPressed && !IsInteracting)
        {
            MainMenuPanel.SetActive(true);
            FreezePlayer();
        }
        */

    }

    public void CloseMainMenu()
    {
        //MainMenuPanel.SetActive(false);
        //UnfreezePlayer();
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    //Make sure the player can't move, and make the cursor visible and confined to the screen
    //This is commented out in this version because it's not needed, but here to let you see what happens in the 3d space
    public void FreezePlayer()
    {
        /*
        IsInteracting = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        */
    }

    //Reverse the above, and also clear our input so we don't flip out suddenly
    //This is commented out in this version because it's not needed, but here to let you see what happens in the 3d space
    public void UnfreezePlayer()
    {
        /*
        IsInteracting = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Input.ResetInputAxes();
        GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        */
    }

    void PopulateGameMenu()
    {
        //Sort the games list alphabetically
        lockpickGames.Sort();
        //To construct the menu of minigames, we go through the list where they've all been added
        foreach (LockpickGame game in lockpickGames)
        {
            //Create a new button prefab
            GameObject newButtonObject = Instantiate<GameObject>(gameButtonPrefab);

            //Set the text on the button to the name of the game
            TextMeshProUGUI buttonText = newButtonObject.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = game.GetFriendlyName();

            //Set up the button so that when it's clicked it launches the game
            Button buttonComponent;
            buttonComponent = newButtonObject.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => { game.BeginLockpicking(this); });

            //Add the button to the list
            newButtonObject.transform.SetParent(gameListContentPanel);
        }
    }


}
