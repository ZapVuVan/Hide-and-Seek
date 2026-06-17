using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WorldUIInput : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask uiLayerMask;

    private PowerCardUI _pressedCard = null;
    private int _trackedFingerId = -1;

    private void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (t.phase == TouchPhase.Began && _trackedFingerId == -1)
            {
                if (IsTouchOnOverlay(t.position))
                {
                    Debug.Log("Bị Overlay chặn!");
                    continue;
                }

                PowerCardUI card = RaycastCard(t.position);
                if (card != null)
                {
                    Debug.Log("Nhấn xuống card: " + card.name);
                    _pressedCard = card;
                    _trackedFingerId = t.fingerId;
                }
                else
                {
                    Debug.Log("Ray không trúng gì!");
                }
            }

            if (t.fingerId == _trackedFingerId && t.phase == TouchPhase.Ended)
            {
                PowerCardUI card = RaycastCard(t.position);
                if (card != null && card == _pressedCard)
                {
                    Debug.Log("Mua thành công!");
                    card.OnClickBuy();
                }
                Reset();
            }
        }
    }

    private bool IsTouchOnOverlay(Vector2 screenPos)
    {
        var pointer = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            Canvas c = r.gameObject.GetComponentInParent<Canvas>();
            if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                return true;
        }
        return false;
    }

    private PowerCardUI RaycastCard(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, uiLayerMask))
            return hit.collider.GetComponentInParent<PowerCardUI>();
        return null;
    }

    private void Reset()
    {
        _pressedCard = null;
        _trackedFingerId = -1;
    }
}