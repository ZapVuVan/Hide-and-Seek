using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HiderInvisibleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image invisibleBar;
    [SerializeField] private TextMeshProUGUI percentText;

    [Header("Target")]
    // ✅ Kéo InvisibleController của local player vào đây trong Inspector
    [SerializeField] private InvisibleController _targetController;

    private void Start()
    {
        if (invisibleBar == null)
            invisibleBar = GetComponent<Image>();
    }

    private void OnEnable()
    {
        InvisibleController.OnInvisibleUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        InvisibleController.OnInvisibleUpdated -= UpdateUI;
    }

    // ✅ Chỉ update khi event đến từ đúng controller của mình
    private void UpdateUI(InvisibleController sender, float fillValue)
    {
        if (sender != _targetController) return;
        if (invisibleBar == null) return;

        float clamped = Mathf.Clamp01(fillValue);
        invisibleBar.fillAmount = clamped;

        if (percentText != null)
        {
            int percent = Mathf.RoundToInt(clamped * 100f);
            percentText.text = percent + "%";
        }
    }

    public void ForceShow()
    {
        if (invisibleBar != null) invisibleBar.enabled = true;
        if (percentText != null) percentText.enabled = true;
    }

    public void ForceHide()
    {
        if (invisibleBar != null) invisibleBar.enabled = false;
        if (percentText != null) percentText.enabled = false;
    }

    public void SetFill(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (invisibleBar != null) invisibleBar.fillAmount = clamped;
        if (percentText != null) percentText.text = Mathf.RoundToInt(clamped * 100f) + "%";
    }

    public void SetBarVisible(bool visible)
    {
        if (invisibleBar != null) invisibleBar.enabled = visible;
        if (percentText != null) percentText.enabled = visible;
    }
}