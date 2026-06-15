using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class NotificationCoin : MonoBehaviour
{
    public static NotificationCoin Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float duration = 1.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void Start()
    {
        Hide();
    }
    public void ShowCoin(int amount,int reason)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine(amount,reason));
    }

    private IEnumerator ShowRoutine(int amount, int reason)
    {
        Show();
        if(reason == 1)
        {
            text.text = $"Sống sót + {amount} coin";
        }
        if(reason == 2)
        {
            text.text = $"Diệt hider + {amount} coin";
        }
        

        yield return new WaitForSeconds(duration);

        Hide();
    }

    public void Show()
    {
        text.gameObject.SetActive(true);
    }
    public void Hide()
    {
        text.gameObject.SetActive(false);
    }
}