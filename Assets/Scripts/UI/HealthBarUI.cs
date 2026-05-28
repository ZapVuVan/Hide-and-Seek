// HealthBarUI.cs
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private void Start()
    {
        var health = GetComponentInParent<Health>();
        if (health != null)
            health.OnHealthChanged += UpdateBar;
    }

    private void UpdateBar(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}