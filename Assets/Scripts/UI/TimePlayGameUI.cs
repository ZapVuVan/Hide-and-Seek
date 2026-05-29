// TimePlayGameUI.cs
using UnityEngine;
using TMPro;

public class TimePlayGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;


    public void Start()
    {
        Hide();
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateTimer(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}