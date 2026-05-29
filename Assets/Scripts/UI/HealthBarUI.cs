// HealthBarUI.cs
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
            fillImage.fillAmount = health.GetHealthPercent();
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
    }
}