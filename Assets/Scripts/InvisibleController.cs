using UnityEngine;

public class InvisibleController : MonoBehaviour
{
    [SerializeField] private Transform targetObj;
    [SerializeField] private HiderInvisibleUI invisibleUI;

    [Header("Settings")]
    [SerializeField] private float fillSpeed = 0.3f;
    [SerializeField] private float drainSpeed = 0.5f;
    [SerializeField] private float speedThreshold = 0.1f;
    [SerializeField] private float fadeSpeed = 3f;

    public float _fillAmount;
    public float _currentAlpha = 1f;
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = targetObj.GetComponentsInChildren<Renderer>();

        if (invisibleUI != null)
        {
            invisibleUI.SetFill(0f);
            invisibleUI.SetBarVisible(false);
        }
    }

    public void UpdateInvisible(float speed)
    {
        if (invisibleUI != null)
            invisibleUI.SetBarVisible(true);

        bool isStanding = speed < speedThreshold;

        _fillAmount += isStanding
            ? fillSpeed * Time.deltaTime
            : -drainSpeed * Time.deltaTime;

        _fillAmount = Mathf.Clamp01(_fillAmount);

        if (invisibleUI != null)
            invisibleUI.SetFill(_fillAmount);

        float targetAlpha = 1f - _fillAmount;

        _currentAlpha = Mathf.Lerp(
            _currentAlpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        ApplyAlpha();
    }

    private void ApplyAlpha()
    {
        foreach (Renderer r in _renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat == null) continue;
                if (!mat.HasProperty("_Color")) continue; // ← thêm dòng này

                Color c = mat.color;
                c.a = _currentAlpha;
                mat.color = c;
            }
        }
    }

    public void ResetInvisible()
    {
        _fillAmount = 0f;
        _currentAlpha = 1f;

        if (invisibleUI != null)
        {
            invisibleUI.SetFill(0f);
            invisibleUI.SetBarVisible(false);
        }

        ApplyAlpha();
    }
}