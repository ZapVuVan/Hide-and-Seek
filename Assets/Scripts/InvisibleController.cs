using UnityEngine;
using System.Collections.Generic;

public class InvisibleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetObj;
    [SerializeField] private HiderInvisibleUI invisibleUI;

    [Header("Settings")]
    [SerializeField] private float fillSpeed = 0.3f;
    [SerializeField] private float drainSpeed = 0.5f;
    [SerializeField] private float speedThreshold = 0.1f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Runtime Info")]
    public float _fillAmount;
    public float _currentAlpha = 1f;

    private List<Material> _cachedMaterials = new List<Material>();
    private bool _isLocalPlayer; // Biến kiểm tra tự động bằng Tag

    private void Awake()
    {
        if (targetObj == null) targetObj = this.transform;

        // Tự động kiểm tra: Nếu Object này hoặc Object cha của nó có Tag là "Player"
        _isLocalPlayer = transform.root.CompareTag("Player") || CompareTag("Player");

        // Cache vật liệu để bản build mượt, tránh lỗi tràn bộ nhớ (Cả Player và Bot đều cần)
        Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            _cachedMaterials.AddRange(r.materials);
        }

        // CHỈ CÓ NHÂN VẬT MANG TAG "Player" mới đi tìm và quản lý UI
        if (_isLocalPlayer)
        {
            if (invisibleUI == null)
            {
                invisibleUI = Object.FindAnyObjectByType<HiderInvisibleUI>();
            }

            if (invisibleUI != null)
            {
                invisibleUI.SetFill(0f);
                invisibleUI.SetBarVisible(false);
            }
        }
    }

    public void UpdateInvisible(float speed)
    {
        // Tính toán trạng thái tàng hình (Cả Player và Bot đều chạy để ẩn mô hình 3D)
        bool isStanding = speed < speedThreshold;

        _fillAmount += isStanding
            ? fillSpeed * Time.deltaTime
            : -drainSpeed * Time.deltaTime;

        _fillAmount = Mathf.Clamp01(_fillAmount);

        // CHỈ CÓ NHÂN VẬT MANG TAG "Player" mới được cập nhật lên thanh UI màn hình
        if (_isLocalPlayer && invisibleUI != null)
        {
            invisibleUI.SetBarVisible(true);
            invisibleUI.SetFill(_fillAmount);
        }

        float targetAlpha = 1f - _fillAmount;
        _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        ApplyAlpha();
    }

    private void ApplyAlpha()
    {
        for (int i = 0; i < _cachedMaterials.Count; i++)
        {
            Material mat = _cachedMaterials[i];
            if (mat == null) continue;

            if (mat.HasProperty("_Color"))
            {
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

        if (_isLocalPlayer && invisibleUI != null)
        {
            invisibleUI.SetFill(0f);
            invisibleUI.SetBarVisible(false);
        }

        ApplyAlpha();
    }
}