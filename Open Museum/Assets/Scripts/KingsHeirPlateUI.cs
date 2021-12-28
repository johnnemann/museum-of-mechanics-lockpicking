using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;


//This is a class to hold the UI representation of a "plate" in the King's Heir game
//It has references to all the UI objects we might be interested in, as well as some gameplay logic data
//All this data is manually assigned in the editor
public class KingsHeirPlateUI : MonoBehaviour
{
    //A unique ID for the plate
    public int plateID;
    //The plate's actual game object
    public GameObject plate;
    //The Button component
    public Button plateButton;
    //Our Line gameobjects
    public List<GameObject> lines;

    //The plate(s) that must be unlocked in order to unlock this one. You only need to include the immediately-preceding plates
    public List<int> requiredPlates;

    //If this is the last plate in the set, mark this as true - used to determine success
    public bool lastPlate = false;

    //Called to retract the bars attached to this plate.
    //This uses the "fill image" property of Unity sprites and tweening to animate retracting and extending the bars
    public void Retract()
    {
        foreach (GameObject line in lines)
        {
            Image lineImage = line.GetComponent<Image>();
            //OPENSOURCE: comment out dotween calls
            //We use DOTween, which is an excellent Unity asset: http://dotween.demigiant.com/index.php 
            lineImage.DOFillAmount(0, 1.0f);
        }
    }

    //Called to reset the lock to the starting position, for failure or re-doing it
    public void ResetLock(bool instant)
    {
        foreach (GameObject line in lines)
        {
            Image lineImage = line.GetComponent<Image>();
            if (instant)
            {
                lineImage.fillAmount = 1.0f;
            }
            else
            {
                //OPENSOURCE: comment out dotween calls
                //We use DOTween, which is an excellent Unity asset: http://dotween.demigiant.com/index.php 
                lineImage.DOFillAmount(1, 1.0f);
            }
        }
    }
}