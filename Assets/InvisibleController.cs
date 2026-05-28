// InvisibleController.cs
using UnityEngine;

public class InvisibleController : MonoBehaviour
{
    [SerializeField] private Transform targetObj;
    [SerializeField] private float fillSpeed = 0.3f;
    [SerializeField] private float drainSpeed = 0.5f;
    [SerializeField] private float speedThreshold = 0.1f;
    [SerializeField] private float fadeSpeed = 2f;

    private float _fillAmount = 0f;
    private float _currentAlpha = 1f;
    private float _targetAlpha = 1f;
    private Renderer[] _renderers;
    private MaterialPropertyBlock _propBlock;

    public float FillAmount => _fillAmount;
    public System.Action<float> OnFillChanged;
    public System.Action<bool> OnBarVisibleChanged;

    private void Awake()
    {
        _renderers = targetObj.GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    public void UpdateInvisible(float speed)
    {
        OnBarVisibleChanged?.Invoke(true);

        bool isStanding = speed < speedThreshold;
        _fillAmount += isStanding
            ? fillSpeed * Time.deltaTime
            : -drainSpeed * Time.deltaTime;

        _fillAmount = Mathf.Clamp01(_fillAmount);
        OnFillChanged?.Invoke(_fillAmount);

        _targetAlpha = 1f - _fillAmount;
        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, fadeSpeed * Time.deltaTime);
        ApplyAlpha();
    }

    public void ResetInvisible()
    {
        _fillAmount = 0f;
        _currentAlpha = 1f;
        _targetAlpha = 1f;
        OnFillChanged?.Invoke(_fillAmount);
        OnBarVisibleChanged?.Invoke(false);
        ApplyAlpha();
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