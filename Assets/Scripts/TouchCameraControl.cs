using UnityEngine;
using UnityEngine.EventSystems;

public class TouchCameraController : MonoBehaviour, IDragHandler
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject headPlayer;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 0.2f;
    [SerializeField] private float sensitivityY = 0.15f; // Dọc thấp hơn ngang

    [Header("Vertical Clamp")]
    [SerializeField] private float minY = -70f;
    [SerializeField] private float maxY = 70f;

    [Header("Max Delta Per Frame")]
    [SerializeField] private float maxDeltaX = 15f;
    [SerializeField] private float maxDeltaY = 8f; // Chặn vẩy tay mạnh theo chiều dọc

    [Header("Dead Zone")]
    [SerializeField] private float deadZone = 2f; // pixel, tránh jitter

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.05f;

    private float rotX;
    private float rotY;
    private bool isFirstCam = false;

    private Vector2 currentVelocity;
    private Vector2 targetDelta;

    public void OnDrag(PointerEventData eventData)
    {
        // Clamp delta tối đa mỗi frame, chống vẩy aim đột ngột
        float deltaX = Mathf.Clamp(eventData.delta.x, -maxDeltaX, maxDeltaX);
        float deltaY = Mathf.Clamp(eventData.delta.y, -maxDeltaY, maxDeltaY);

        // Dead zone: bỏ qua delta quá nhỏ (tránh jitter)
        if (Mathf.Abs(deltaX) < deadZone) deltaX = 0f;
        if (Mathf.Abs(deltaY) < deadZone) deltaY = 0f;

        float mouseX = (deltaX / Screen.width) * sensitivityX * 100f;
        float mouseY = (deltaY / Screen.height) * sensitivityY * 100f;

        // Smooth delta để tránh spike đột ngột
        targetDelta = Vector2.SmoothDamp(
            targetDelta,
            new Vector2(mouseX, mouseY),
            ref currentVelocity,
            smoothTime
        );

        rotY += targetDelta.x;
        rotX -= targetDelta.y;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        orientation.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    public void TransitionToFirstPerson()
    {
        isFirstCam = true;
        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.zero;

        rotX = orientation.eulerAngles.x;
        rotY = orientation.eulerAngles.y;
    }

    public void TransitionToThirdPerson()
    {
        isFirstCam = false;
        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.one;

        rotX = orientation.eulerAngles.x;
        rotY = orientation.eulerAngles.y;
    }
}