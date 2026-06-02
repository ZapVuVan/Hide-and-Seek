// TimeHiderUI.cs
using UnityEngine;
using TMPro;

public class TimeHiderUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    public void Start()
    {
            Hide();
    }
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void UpdateTimer(float timeLeft)
    {
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }
}