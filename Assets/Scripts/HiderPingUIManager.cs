using System.Collections.Generic;
using UnityEngine;

public class HiderPingUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform iconPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;

    private List<(RoleComponent hider, GameObject icon)> activeIcons = new();
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }
    public void SetHiders(List<RoleComponent> hiders)
    {
        Clear();

        foreach (var hider in hiders)
        {
            var icon = Instantiate(iconPrefab, canvas.transform);
            icon.gameObject.SetActive(false);
            activeIcons.Add((hider, icon.gameObject));
        }
    }

    private void Update()
    {
        foreach (var (hider, icon) in activeIcons)
        {
            if (hider == null)
            {
                icon.SetActive(false);
                continue;
            }

            Vector3 screenPos = mainCamera.WorldToScreenPoint(hider.transform.position);

            bool isVisible = screenPos.z > 0 &&
                             screenPos.x > 0 && screenPos.x < Screen.width &&
                             screenPos.y > 0 && screenPos.y < Screen.height;

            icon.SetActive(isVisible);

            if (isVisible)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    screenPos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                    out Vector2 localPos
                );
                localPos.y += 20; // Đẩy icon lên trên một chút so với vị trí hider
                icon.GetComponent<RectTransform>().localPosition = localPos;
            }
        }
    }

    public void Clear()
    {
        foreach (var (_, icon) in activeIcons)
            if (icon != null) Destroy(icon);

        activeIcons.Clear();
    }
}