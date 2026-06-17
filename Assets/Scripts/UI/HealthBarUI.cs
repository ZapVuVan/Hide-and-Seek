using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Health health;

    private void Start()
    {
        health = FindObjectOfType<PlayerController>().GetComponent<Health>();

        if (health != null)
        {
            UpdateBar(null, health.GetHealthPercent());
            health.OnHealthChanged += UpdateBar;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateBar;
    }

    private void UpdateBar(object sender, float percent)
    {
        fillImage.fillAmount = percent;

        if (percent > 0.7f)
            fillImage.color = Color.green;
        else if (percent > 0.4f)
            fillImage.color = Color.yellow;
        else if (percent > 0.2f)
            fillImage.color = new Color(1f, 0.5f, 0f); // Cam
        else
            fillImage.color = Color.red;
    }
}