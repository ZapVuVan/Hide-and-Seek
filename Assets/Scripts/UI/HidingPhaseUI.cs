// HidingPhaseUI.cs
using UnityEngine;
using TMPro;

public class HidingPhaseUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI timerText;

    public void Start()
    {
        Hide();
    }
    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void UpdateTimer(float timeLeft)
    {
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }
}