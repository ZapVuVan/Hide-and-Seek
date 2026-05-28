using UnityEngine;

public class HiderInvisibleController : MonoBehaviour
{
    [Header("--- THAM CHIẾU ---")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private HiderInvisibleUI invisibleUI;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Outline outline;

    [Header("--- CẤU HÌNH ---")]
    [SerializeField] private float fillSpeed = 0.3f;
    [SerializeField] private float drainSpeed = 0.5f;
    [SerializeField] private float speedThreshold = 0.1f;
    [SerializeField] private float fadeSpeed = 2f;

    private float _fillAmount = 1f;
    private float _currentAlpha = 1f;
    private float _targetAlpha = 1f;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        _renderers = playerObj.GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (playerController.GetCurrentState() is not PlayerHiderState)
        {
            _fillAmount = 0f;
            invisibleUI.SetFill(_fillAmount);
            invisibleUI.SetBarVisible(false);
            _targetAlpha = 1f;
            if (outline != null) outline.enabled = false;
            ApplyAlpha();
            return;
        }

        invisibleUI.SetBarVisible(true);

        float speed = playerMovement.GetSpeed();
        bool isStanding = speed < speedThreshold;

        if (isStanding)
            _fillAmount += fillSpeed * Time.deltaTime;
        else
            _fillAmount -= drainSpeed * Time.deltaTime;

        _fillAmount = Mathf.Clamp01(_fillAmount);
        invisibleUI.SetFill(_fillAmount);

        _targetAlpha = 1f - _fillAmount;
        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, fadeSpeed * Time.deltaTime);
        ApplyAlpha();

        // Bật outline khi gần tàng hình hoàn toàn
        if (outline != null)
            outline.enabled = _fillAmount > 0.8f;
    }

    private void ApplyAlpha()
    {
        foreach (Renderer r in _renderers)
        {
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_Color", new Color(1f, 1f, 1f, _currentAlpha));
            r.SetPropertyBlock(_propBlock);
        }
    }
}