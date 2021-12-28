using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class Readable : Frobbable
{

    public ReadingHandler readingHandler;

    public string SignName = "";

    [SerializeField]
    InfoData signData;

    //[TextArea]
    //public string ReadText = "";

    Player myPlayer;

    public override void OnFrob(Player player)
    {
        player.FreezePlayer();
        BeginReading();
        myPlayer = player;

    }

    public virtual void BeginReading()
    {
        readingHandler.OpenReadingPanel(signData.SignText, this);
    }

    public virtual void EndReading()
    {
        myPlayer.UnfreezePlayer();
        
    }

    public InfoData GetInfoData()
    {
        return signData;
    }

    public void SetInfoData(InfoData id)
    {
        signData = id;
    }
}
