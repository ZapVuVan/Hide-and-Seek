using UnityEngine;
using System;
using System.Collections.Generic;

public class InvisibleController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fillSpeed = 0.66f;
    [SerializeField] private float drainSpeed = 0.14f;
    [SerializeField] private float speedThreshold = 0.1f;

    // 🔥 Event chỉ cần truyền đúng giá trị fillAmount hiện tại
    public static event Action<float> OnInvisibleUpdated;

    private RoleComponent _role;
    private List<Material> _materials = new();
    public float _fillAmount;

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
        if (!IsHider())
        {
            // Nếu không phải Hider, ép thanh fill về 0
            OnInvisibleUpdated?.Invoke(0f);
            return;
        }

        bool isStanding = speed < speedThreshold;
        float target = isStanding ? 1f : 0f;
        float rate = isStanding ? fillSpeed : drainSpeed;

        _fillAmount = Mathf.MoveTowards(_fillAmount, target, rate * Time.deltaTime);
        _fillAmount = Mathf.Clamp01(_fillAmount);

        ApplyFade();

        // 🔥 Bắn sự kiện cập nhật fill
        OnInvisibleUpdated?.Invoke(_fillAmount);
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

    public void ResetInvisible()
    {
        _fillAmount = 0f;
        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            Color c = mat.color;
            c.a = 1f;
            mat.color = c;
            mat.SetFloat("_Cutoff", 0.001f);
        }

        OnInvisibleUpdated?.Invoke(0f);
    }
}