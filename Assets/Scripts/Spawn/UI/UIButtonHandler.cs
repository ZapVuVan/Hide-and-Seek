using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Gắn 1 lần vào scene — tự động xử lý hiệu ứng press cho MỌI Button.
/// Không cần chạm vào prefab, không cần script phụ.
/// </summary>
public class UIButtonHandler : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale")]
    public float pressedScale = 0.91f;
    public float pressDuration = 0.07f;
    public Ease pressEase = Ease.OutQuad;
    public float releaseDuration = 0.20f;
    public Ease releaseEase = Ease.OutBack;

    [Header("Overlay")]
    [Range(0f, 1f)]
    public float overlayAlpha = 0.26f;
    public float overlayInDuration = 0.05f;
    public float overlayOutDuration = 0.16f;

    // ── State tracking ──
    private RectTransform _pressedRect;
    private Image _activeOverlay;
    private Tween _scaleTween;
    private Tween _overlayTween;

    // Cache overlay theo button để tái dụng
    private readonly Dictionary<GameObject, Image> _overlayCache = new();

    // ────────────────────────────────────────────
    //  Unity gọi 3 interface này khi UIButtonHandler
    //  nằm trên cùng EventSystem — nhưng ta cần nó
    //  nhận event từ MỌI button, không chỉ GO của nó.
    //  Trick: đăng ký vào EventSystem.current.
    // ────────────────────────────────────────────

    void OnEnable() => EventSystem.current?.SetSelectedGameObject(null);
    void OnDisable() => Release();

    // ── EventSystem tick — phát hiện pointer down trên bất kỳ Button nào ──
    void Update()
    {
        // Không xử lý nếu không có input
        if (!Input.GetMouseButtonDown(0) && !HasTouchBegan()) return;

        var go = GetPointerHit();
        if (go == null) return;

        // Tìm Button trên GO hoặc parent gần nhất
        var btn = go.GetComponentInParent<Button>();
        if (btn == null || !btn.interactable) return;

        BeginPress(btn.GetComponent<RectTransform>() ?? btn.transform as RectTransform, go);
    }

    void BeginPress(RectTransform rt, GameObject go)
    {
        if (rt == null) return;
        _pressedRect = rt;

        // Scale xuống
        _scaleTween?.Kill();
        _scaleTween = rt.DOScale(pressedScale, pressDuration)
                        .SetEase(pressEase)
                        .SetUpdate(true);

        // Overlay
        _activeOverlay = GetOrCreateOverlay(go);
        _overlayTween?.Kill();
        _overlayTween = _activeOverlay
                        .DOFade(overlayAlpha, overlayInDuration)
                        .SetUpdate(true);
    }

    // ── Pointer Up / Exit — thả ──
    public void OnPointerDown(PointerEventData e) { }   // handled in Update
    public void OnPointerUp(PointerEventData e) => Release();
    public void OnPointerExit(PointerEventData e) => Release();

    void Release()
    {
        if (_pressedRect == null) return;

        _scaleTween?.Kill();
        _scaleTween = _pressedRect
                      .DOScale(1f, releaseDuration)
                      .SetEase(releaseEase)
                      .SetUpdate(true)
                      .OnComplete(() => _pressedRect = null);

        if (_activeOverlay != null)
        {
            var ov = _activeOverlay;
            _overlayTween?.Kill();
            _overlayTween = ov.DOFade(0f, overlayOutDuration)
                              .SetEase(Ease.OutQuad)
                              .SetUpdate(true);
            _activeOverlay = null;
        }
    }

    // ── Tạo / lấy overlay Image cho button ──
    Image GetOrCreateOverlay(GameObject buttonGO)
    {
        if (_overlayCache.TryGetValue(buttonGO, out var cached) && cached != null)
            return cached;

        var go = new GameObject("__Overlay", typeof(Image));
        go.transform.SetParent(buttonGO.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = false;
        go.transform.SetAsLastSibling();

        _overlayCache[buttonGO] = img;
        return img;
    }

    // ── Helpers ──
    static GameObject GetPointerHit()
    {
        var es = EventSystem.current;
        if (es == null) return null;

        var data = new PointerEventData(es) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        es.RaycastAll(data, results);

        return results.Count > 0 ? results[0].gameObject : null;
    }

    static bool HasTouchBegan()
    {
        foreach (Touch t in Input.touches)
            if (t.phase == TouchPhase.Began) return true;
        return false;
    }

    void OnDestroy()
    {
        _scaleTween?.Kill();
        _overlayTween?.Kill();
        DOTween.Kill(_pressedRect);
    }
}