using UnityEngine;
using TMPro;

public class TimePlayGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        GameManager.Instance.OnGameStart += Show;
        GameManager.Instance.OnPlayingTimeUpdate += UpdateTimer;
        GameManager.Instance.OnGameFinish += Hide;

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

    private void UpdateTimer(float timeLeft)
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        timerText.text = $"{minutes:0}:{seconds:00}";
    }
}