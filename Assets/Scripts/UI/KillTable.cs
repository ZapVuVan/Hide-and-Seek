using UnityEngine;
using TMPro;
using System.Collections;

public class KillTable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killerText;
    [SerializeField] private TextMeshProUGUI victimText;

    [SerializeField] private GameObject killUI;
    [SerializeField] private float displayTime = 3f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        killerText.text = "";
        victimText.text = "";
        killUI.SetActive(false);
    }

    public void OnKilled(GameObject killer, GameObject victim)
    {
        string killerName = killer != null ? killer.name : "Unknown";
        string victimName = victim.name;
        killerText.text = $"{killerName}";
        victimText.text = $"killed {victimName}";

        killUI.SetActive(true);

        // Nếu đang đếm thì reset lại
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        killUI.SetActive(false);
    }
}