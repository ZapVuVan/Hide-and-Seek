// RoleRevealUI.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class RoleRevealUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private float spinDuration = 2.5f; // thời gian chạy loạn
    [SerializeField] private float slowDuration = 0.5f;  // thời gian chậm dần

    private string[] labels = { "HIDER", "SEEKER" };

    private void Start() => Hide();
    public void Hide() => panel.SetActive(false);

    public IEnumerator PlayReveal(GameRole finalRole)
    {

        panel.SetActive(true);
        string finalLabel = finalRole == GameRole.Seeker ? "SEEKER" : "HIDER";

 
        float elapsed = 0f;
        float interval = 0.08f;
        int index = 0;
        while (elapsed < spinDuration)
        {
            roleText.text = labels[index % 2];
            index++;
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        elapsed = 0f;
        while (elapsed < slowDuration)
        {
            float t = elapsed / slowDuration;
            interval = Mathf.Lerp(0.08f, 0.4f, t);
            roleText.text = labels[index % 2];
            index++;
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        roleText.text = finalLabel;
        yield return new WaitForSeconds(1f);

        panel.SetActive(false);
    }
}