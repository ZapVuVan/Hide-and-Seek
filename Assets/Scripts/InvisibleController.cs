using UnityEngine;
using System;
using System.Collections.Generic;

public class InvisibleController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fillSpeed = 0.66f;
    [SerializeField] private float drainSpeed = 0.14f;
    [SerializeField] private float speedThreshold = 0.1f;

    [Header("Seeker Proximity")]
    [SerializeField] private float seekerDetectRadius = 5f;
    [SerializeField] private float proximityTransitionSpeed = 2f;
    private const float ProximityMaxFill = 0.9f;

    // ✅ Truyền kèm instance để UI lọc đúng controller
    public static event Action<InvisibleController, float> OnInvisibleUpdated;

    private RoleComponent _role;
    private List<Material> _materials = new();
    public float _fillAmount;
    private bool _isDead = false;
    private float _currentMaxFill = 1f;

    private void Awake()
    {
        _role = GetComponentInParent<RoleComponent>();
        foreach (var r in GetComponentsInChildren<Renderer>())
            _materials.AddRange(r.materials);
    }

    private bool IsHider()
    {
        return _role != null && _role.Role == GameRole.Hider;
    }

    private bool IsSeekerNearby()
    {
        var allRoles = FindObjectsByType<RoleComponent>(FindObjectsSortMode.None);
        foreach (var role in allRoles)
        {
            if (role == _role) continue;
            if (role.Role != GameRole.Seeker) continue;
            float dist = Vector3.Distance(transform.position, role.transform.position);
            if (dist <= seekerDetectRadius) return true;
        }
        return false;
    }

    public void SetTransparentMode()
    {
        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            mat.SetFloat("_Surface", 1f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    public void SetOpaqueMode()
    {
        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            mat.SetFloat("_Surface", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
    }

    public void UpdateInvisible(float speed)
    {
        if (_isDead) return;

        if (!IsHider())
        {
            if (_fillAmount != 0f)
            {
                _fillAmount = 0f;
                OnInvisibleUpdated?.Invoke(this, 0f);
            }
            return;
        }

        bool isStanding = speed < speedThreshold;
        float target = isStanding ? 1f : 0f;
        float rate = isStanding ? fillSpeed : drainSpeed;

        float newFill = Mathf.MoveTowards(_fillAmount, target, rate * Time.deltaTime);
        newFill = Mathf.Clamp01(newFill);

        // ✅ Chỉ clamp khi fill đã đạt gần 100% và Seeker đang gần
        if (newFill >= 0.999f && IsSeekerNearby())
        {
            _currentMaxFill = Mathf.MoveTowards(_currentMaxFill, ProximityMaxFill, proximityTransitionSpeed * Time.deltaTime);
            newFill = Mathf.Min(newFill, _currentMaxFill);
        }
        else
        {
            _currentMaxFill = 1f;
        }

        if (!Mathf.Approximately(newFill, _fillAmount))
        {
            _fillAmount = newFill;
            ApplyFade();
            OnInvisibleUpdated?.Invoke(this, _fillAmount);
        }
    }

    private void ApplyFade()
    {
        float alpha = 1f - _fillAmount;
        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
            mat.SetFloat("_Cutoff", 0.001f);
        }
    }

    public void OnDead()
    {
        _isDead = true;
        _fillAmount = 1f;
        ApplyFade();
        OnInvisibleUpdated?.Invoke(this, _fillAmount);
    }

    public void ResetInvisible()
    {
        _isDead = false;
        _fillAmount = 0f;
        _currentMaxFill = 1f;

        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            Color c = mat.color;
            c.a = 1f;
            mat.color = c;
            mat.SetFloat("_Cutoff", 0.001f);
        }

        OnInvisibleUpdated?.Invoke(this, 0f);
    }
}