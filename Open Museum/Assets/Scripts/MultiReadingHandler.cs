using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiReadingHandler : MonoBehaviour
{
    public GameObject ReadingPanel;
    public TextMeshProUGUI ReadingText;

    public TextMeshProUGUI TitleText;

    string BackgroundText, MechanicsText, AnalysisText;

    MultiReadable CurrentReadable;

    public Button defaultSelectedButton;

    public void OpenReadingPanel(string text, MultiReadable readable)
    {
        ReadingPanel.SetActive(true);
        ReadingText.text = text;
        CurrentReadable = readable;
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
    }

    public void SetReadingText(string text)
    {

    }

    public void CloseReadingPanel()
    {
        ReadingPanel.SetActive(false);
        CurrentReadable.EndReading();
    }

    public void OpenBackgroundText()
    {
        ReadingText.text = BackgroundText;
    }

    public void OpenMechanicsText()
    {
        ReadingText.text = MechanicsText;
    }

    public void OpenAnalysisText()
    {
        ReadingText.text = AnalysisText;
    }

    public void SetBackgroundText(string text)
    {
        BackgroundText = text;
    }

    public void SetMechanicsText(string text)
    {
        MechanicsText = text;
    }

    public void SetAnalysisText(string text)
    {
        AnalysisText = text;
    }

    public void SetTitle(string text)
    {
        TitleText.text = text;
    }
}
