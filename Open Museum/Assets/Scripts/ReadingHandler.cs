using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ReadingHandler : MonoBehaviour
{
    public GameObject ReadingPanel;
    public TextMeshProUGUI ReadingText;
    public Button CloseButton;

    Readable currentReadable;

    public void OpenReadingPanel(string text, Readable readable)
    {
        ReadingPanel.SetActive(true);
        ReadingText.text = text;
        currentReadable = readable;
    }

    public void SetReadingText(string text)
    {
        ReadingText.text = text;
    }

    public void CloseReadingPanel()
    {
        ReadingPanel.SetActive(false);
        currentReadable.EndReading();
    }
}
