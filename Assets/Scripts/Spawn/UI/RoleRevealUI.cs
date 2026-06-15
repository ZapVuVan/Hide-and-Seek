using System.Collections;
using UnityEngine;
using TMPro;

public class RoleRevealUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private float slowDuration = 1f;

    private string[] labels = { "HIDER", "SEEKER" };

    private Color hiderColor = Color.blue;
    private Color seekerColor = Color.red;

    private void Start() => Hide();
    public void Hide() => panel.SetActive(false);

    private void SetLabelWithColor(string label)
    {
        roleText.text = label;
        roleText.color = label == "SEEKER" ? seekerColor : hiderColor;
    }

    public IEnumerator PlayReveal(GameRole finalRole)
    {
        panel.SetActive(true);
        string finalLabel = finalRole == GameRole.Seeker ? "SEEKER" : "HIDER";

        float elapsed = 0f;
        float interval = 0.08f;
        int index = 0;

        while (elapsed < spinDuration)
        {
            SetLabelWithColor(labels[index % 2]);
            index++;
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        elapsed = 0f;
        while (elapsed < slowDuration)
        {
            float t = elapsed / slowDuration;
            interval = Mathf.Lerp(0.08f, 0.4f, t);
            SetLabelWithColor(labels[index % 2]);
            index++;
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        SetLabelWithColor(finalLabel);
        yield return new WaitForSeconds(1f);
        panel.SetActive(false);
    }
}