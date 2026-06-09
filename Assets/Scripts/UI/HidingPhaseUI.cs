using System.Collections;
using TMPro;
using UnityEngine;

public class HidingPhaseUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject seekerPanel;
    [SerializeField] private GameObject hiderPanel;

    [Header("Timer Text")]
    [SerializeField] private TextMeshProUGUI seekerTimerText;
    [SerializeField] private TextMeshProUGUI hiderTimerText;

    private Coroutine timerCoroutine;


    private void Start()
    {
        GameManager.Instance.OnHidingPhaseStart += Show;
        Hide();
    }

    public void Show(float duration, bool isSeeker)
    {
        seekerPanel.SetActive(isSeeker);
        hiderPanel.SetActive(!isSeeker);

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(UpdateTimer(duration, isSeeker));
    }

    public void Hide()
    {
        seekerPanel.SetActive(false);
        hiderPanel.SetActive(false);

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator UpdateTimer(float duration, bool isSeeker)
    {
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            string timeText = Mathf.CeilToInt(timeLeft).ToString();

            if (isSeeker)
                seekerTimerText.text = timeText;
            else
                hiderTimerText.text = timeText;

            timeLeft -= Time.deltaTime;

            yield return null;
        }

        if (isSeeker)
            seekerTimerText.text = "0";
        else
            hiderTimerText.text = "0";

        Hide();
    }
}