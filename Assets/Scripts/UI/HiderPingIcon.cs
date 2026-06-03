using UnityEngine;

public class HiderPingIcon : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform icon;
    [SerializeField] private Canvas canvas;

    private Camera mainCam;
    private RectTransform canvasRect;
    private Transform target;

    private bool isShowing;

    private void Awake()
    {
        icon.gameObject.SetActive(false);
    }

    private void Start()
    {
        mainCam = Camera.main;
        canvasRect = canvas.GetComponent<RectTransform>();
        target = transform;
    }

    public void Show()
    {
        isShowing = true;
        icon.gameObject.SetActive(true);
    }

    public void Hide()
    {
        isShowing = false;
        icon.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!isShowing) return;
        if (target == null || mainCam == null) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);

        // nếu sau camera
        if (screenPos.z < 0)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 localPos
        );

        icon.anchoredPosition = localPos;
    }
}