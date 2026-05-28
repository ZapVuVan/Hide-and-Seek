using UnityEngine;
using UnityEngine.EventSystems;

public class TouchCameraController : MonoBehaviour, IDragHandler
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform orientation;

    [SerializeField] private Transform cameraTarget;

    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 0.2f;
    [SerializeField] private float sensitivityY = 0.2f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minY = -70f;
    [SerializeField] private float maxY = 70f;

    private float rotX;
    private float rotY;

    private void Start()
    {
        rotY = playerRoot.eulerAngles.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float mouseX = eventData.delta.x * sensitivityX;
        float mouseY = eventData.delta.y * sensitivityY;

        if (playerController.IsFirstPerson())
        {
            HandleFirstPerson(mouseX, mouseY);
        }
        else
        {
            HandleThirdPerson(mouseX, mouseY);
        }
    }

    // ================= FPS =================
    private void HandleFirstPerson(float mouseX, float mouseY)
    {
        // ----- YAW -----
        rotY += mouseX;

        // FPS xoay player thật
        playerRoot.rotation =
            Quaternion.Euler(0f, rotY, 0f);

        // Orientation follow player
        orientation.rotation =
            Quaternion.Euler(0f, rotY, 0f);

        // ----- PITCH -----
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        // Camera nhìn lên xuống
        cameraTarget.localRotation =
            Quaternion.Euler(rotX, 0f, 0f);
    }

    // ================= TPS =================
    private void HandleThirdPerson(float mouseX, float mouseY)
    {
        rotY += mouseX;

        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        // TPS giữ kiểu cũ
        orientation.rotation =
            Quaternion.Euler(rotX, rotY, 0f);
    }

    public void SetRotation(float x, float y)
    {
        rotX = x;
        rotY = y;
    }
}