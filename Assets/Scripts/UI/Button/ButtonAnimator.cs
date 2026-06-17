using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonAnimator : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float pressedScale = 0.90f;
    [SerializeField] private float scaleDuration = 0.10f;
    [SerializeField] private Ease scaleEase = Ease.OutQuad;

    [Header("Fade")]
    [SerializeField] private float pressedAlpha = 0.75f;
    [SerializeField] private float fadeDuration = 0.08f;

    private CanvasGroup _cg;
    private bool _isPressed = false;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();

        var rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    void OnEnable()
    {
        _isPressed = false;
    }

    public void OnPointerDown(PointerEventData _)
    {
        _isPressed = true;

        transform.DOKill();
        _cg.DOKill();

        transform.DOScale(Vector3.one * pressedScale, scaleDuration)
                 .SetEase(scaleEase)
                 .SetUpdate(true);

        _cg.DOFade(pressedAlpha, fadeDuration)
           .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData _)
    {
        _isPressed = false;
        Release();
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (!_isPressed) return;
        _isPressed = false;
        Release();
    }

    private void Release()
    {
        transform.DOKill();
        _cg.DOKill();

        transform.DOScale(Vector3.one, scaleDuration * 1.5f)
                 .SetEase(Ease.OutBack)
                 .SetUpdate(true);

        _cg.DOFade(1f, fadeDuration)
           .SetUpdate(true);
    }

    void OnDisable()
    {
        _isPressed = false;
        transform.DOKill();
        _cg.DOKill();
        transform.localScale = Vector3.one;
        _cg.alpha = 1f;
    }
}