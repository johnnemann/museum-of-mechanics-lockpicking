using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


//This class holds references to the UI elements that make up a node and handles interactions
public class MassEffect2Node : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    //The image that we can set the sprite of to change our appearance
    public Image nodeImage;

    //References to the sprite that shows when we're hidden and the one that shows when we're flipped over
    public Sprite hiddenSprite;
    public Sprite flippedSprite;

    //The unique ID of this node
    public int nodeID;

    //A reference to the controlling game
    public MassEffect2LockpickGame lockpickGame;

    //How long the node stays flipped
    public float flipTimer = 2.5f;
    float elapsedFlipTimer = 0.0f;
    bool flipTimerActive = false;

    //When the player selects this node
    public void OnSelect(BaseEventData eventData)
    {
        //flip to display side, start countdown timer
        ShowSprite();
        elapsedFlipTimer = 0.0f;
        flipTimerActive = true;
    }

    //When the player decides to lock this sprite to the flipped side and try to find its match (or this is the match)
    public void LockSprite()
    {
        //lock object flipped
        ShowSprite();
        flipTimerActive = false;
        lockpickGame.SpriteClicked(this);
    }

    void Update()
    {
        //countdown timer - when this expires, flip the sprite back to the hidden side
        if (flipTimerActive)
        {
            elapsedFlipTimer += Time.deltaTime;
            if (elapsedFlipTimer >= flipTimer)
            {
                flipTimerActive = false;
                HideSprite();
            }
        }
    }

    //Just set the appropriate sprite
    public void ShowSprite()
    {
        nodeImage.sprite = flippedSprite;
    }

    public void HideSprite()
    {
        nodeImage.sprite = hiddenSprite;
    }

    //Handle mouse interaction - on enter we select, on click we lock, and on exit we reset
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LockSprite();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        elapsedFlipTimer = flipTimer;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
