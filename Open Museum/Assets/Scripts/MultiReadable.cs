using UnityEngine;
using System.Collections;
using TMPro;

public class MultiReadable : Frobbable
{

    public string SignName = "";

    [SerializeField]
    SignData signData;

    Player myPlayer;

    public MultiReadingHandler readingHandler;

    public SignLoader signLoader;

    public override void OnFrob(Player player)
    {
        player.FreezePlayer();
        myPlayer = player;
        /*
        if (signLoader != null)
        {
            signData = signLoader.GetDataForSign(SignName);
            Debug.Log("Loaded sign for " + SignName);
        }
        */
        BeginReading();
    }

    public virtual void BeginReading()
    {
        readingHandler.OpenReadingPanel(signData.BackgroundText, this);
        readingHandler.SetBackgroundText(signData.BackgroundText);
        readingHandler.SetMechanicsText(signData.MechanicsText);
        readingHandler.SetAnalysisText(signData.AnalysisText);
        readingHandler.SetTitle(signData.TitleText);
    }

    public virtual void EndReading()
    {
        myPlayer.UnfreezePlayer();
    }


    public SignData GetSignData()
    {
        return signData;
    }

    public void SetSignData(SignData sd)
    {
        signData = sd;
    }
}
