using UnityEngine;

public class WorldUIInput : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask uiLayerMask;

    private void Update()
    {
        bool triggered = false;
        Vector2 inputPos = Vector2.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            triggered = true;
            inputPos = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            triggered = true;
            inputPos = Input.mousePosition;
        }

        if (!triggered) return;

        Ray ray = cam.ScreenPointToRay(inputPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, uiLayerMask))
        {
            PowerCardUI card = hit.collider.GetComponentInParent<PowerCardUI>();
            if (card != null) card.OnClickBuy();
        }
    }
}