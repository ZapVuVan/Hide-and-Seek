using UnityEngine;
using UnityEngine.EventSystems;

public class TouchCameraController : MonoBehaviour, IDragHandler
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject headPlayer;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 0.2f;
    [SerializeField] private float sensitivityY = 0.15f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minY = -70f;
    [SerializeField] private float maxY = 70f;

    [Header("Dead Zone")]
    [SerializeField] private float deadZone = 1f;

    private float rotX;
    private float rotY;

    private bool isFirstCam = false;

    private void Start()
    {
        SyncRotation();
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = eventData.delta.x;
        float deltaY = eventData.delta.y;

        // Bỏ qua rung nhẹ
        if (Mathf.Abs(deltaX) < deadZone)
            deltaX = 0f;

        if (Mathf.Abs(deltaY) < deadZone)
            deltaY = 0f;

        // FPS mobile: dùng trực tiếp delta
        rotY += deltaX * sensitivityX;

        // Dọc thấp hơn ngang để tránh vẩy lên trời
        rotX -= deltaY * sensitivityY;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        orientation.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    public void TransitionToFirstPerson()
    {
        isFirstCam = true;

        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.zero;

        SyncRotation();
    }

    public void TransitionToThirdPerson()
    {
        isFirstCam = false;

        if (headPlayer != null)
            headPlayer.transform.localScale = Vector3.one;

        SyncRotation();
    }

    private void SyncRotation()
    {
        Vector3 angles = orientation.eulerAngles;

        rotY = angles.y;

        rotX = angles.x;

        // Chuyển từ 0~360 sang -180~180
        if (rotX > 180f)
            rotX -= 360f;
    }
}