using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class SignData
{
    public string SignName = "";

    [TextArea]
    public string TitleText = "";

    [TextArea]
    public string BackgroundText = "";

    [TextArea]
    public string MechanicsText = "";

    [TextArea]
    public string AnalysisText = "";

}

//For the single-screen signs such as the intro and credits signs
[System.Serializable]
public class InfoData
{
    public string SignName = "";

    [TextArea]
    public string SignText = "";
}

public class SignLoader : MonoBehaviour
{
    private SignData[] signs;

    private InfoData[] info;

    void Awake()
    {
        //LoadSignData();
        //WriteSignData();
        //WriteSingleSignData();
    }

    public void LoadSignData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SignData[]));
        using (StreamReader streamReader = new StreamReader("Assets/LockpickingSigns.xml"))
        {
            signs = (SignData[])serializer.Deserialize(streamReader);
        }
    }

    public void LoadInfoData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(InfoData[]));
        using (StreamReader streamReader = new StreamReader("Assets/InfoSigns.xml"))
        {
            info = (InfoData[])serializer.Deserialize(streamReader);
        }
    }

    public void AssignSignData()
    {
        MultiReadable[] allSigns = FindObjectsOfType<MultiReadable>();
        foreach (MultiReadable mr in allSigns)
        {
            mr.SetSignData(GetDataForSign(mr.SignName));
        }
    }

    public void AssignInfoData()
    {
        Readable[] allSigns = FindObjectsOfType<Readable>();
        foreach (Readable r in allSigns)
        {
            r.SetInfoData(GetInfoForSign(r.SignName));
        }
    }

    public SignData GetDataForSign(string SignName)
    {
        var sign = signs.Where(s => s.SignName == SignName).FirstOrDefault();
        return sign;
    }

    public InfoData GetInfoForSign(string SignName)
    {
        var sign = info.Where(s => s.SignName == SignName).FirstOrDefault();
        return sign;
    }

    //This is intended just to get the data into the XML file the first time from the in-editor fields I've made
    public void WriteSignData()
    {
        List<SignData> signDataToWrite = new List<SignData>();
        MultiReadable[] allSigns = FindObjectsOfType<MultiReadable>();
        foreach (MultiReadable mr in allSigns)
        {
            signDataToWrite.Add(mr.GetSignData());
        }

        XmlSerializer serializer = new XmlSerializer(typeof(SignData[]));
        using (StreamWriter streamWriter = new StreamWriter("Assets/LockpickingSigns.xml"))
        {
            serializer.Serialize(streamWriter, signDataToWrite.ToArray());
        }
    }
    
    public void WriteSingleSignData()
    {
        List<InfoData> signDatatoWrite = new List<InfoData>();
        Readable[] allSigns = FindObjectsOfType<Readable>();
        foreach (Readable readable in allSigns)
        {
            signDatatoWrite.Add(readable.GetInfoData());
        }

        XmlSerializer serializer = new XmlSerializer(typeof(InfoData[]));
        using (StreamWriter streamWriter = new StreamWriter("Assets/InfoSigns.xml"))
        {
            serializer.Serialize(streamWriter, signDatatoWrite.ToArray());
        }
    }
}
