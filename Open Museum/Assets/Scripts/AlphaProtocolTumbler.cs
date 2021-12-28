﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//This class provides an easy place to change the UI elements that make up a tumbler object in the minigame
public class AlphaProtocolTumbler : MonoBehaviour
{
    //Images for our top and bottom part - they always add up to 1
    public Image topTumbler;
    public Image bottomTumbler;

    //The little arrow that points to this tumbler when it's the active one
    public GameObject selectorImage;

    //Colors for the two halves as well as the color when we're successfully locked in place
    public Color topColor;
    public Color bottomColor;
    public Color successColor;

    //this sets the size of the bottom half, based on what's generated by the game. This determines the target for the player, visually
    //It also sets the top half to be the rest of the size. Probably we could just make one image overlap the other, instead (i.e. have the 
    //"top half" color be a full-size image and the bottom half color sitting on top of that in the heirarchy, giving the same basic look - although
    //maybe not if they're both alpha'd?)
    public void SetBottomTumblerSize(float percentage)
    {
        bottomTumbler.fillAmount = percentage;
        topTumbler.fillAmount = 1f - percentage;
    }

    //Set us back to the original colors
    public void ResetColors()
    {
        bottomTumbler.color = bottomColor;
        topTumbler.color = topColor;
    }

    //Turn the whole image into the success color
    public void SetSuccessColor()
    {
        bottomTumbler.color = successColor;
        topTumbler.color = successColor;
    }
}
