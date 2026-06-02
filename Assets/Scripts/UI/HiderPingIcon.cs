using UnityEngine;

public class HiderPingIcon : MonoBehaviour
{
    [SerializeField] private GameObject iconVisual; // Kéo Quad vào đây

    private Camera playerCamera; // Khai báo camera chính cụ thể

    private void Awake()
    {
        iconVisual.SetActive(false);
    }

    private void Start()
    {
        // Cách lấy Camera chính an toàn nhất: 
        // Tìm Camera được gắn trên cùng GameObject với PlayerController
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        // Nếu không thấy, mới dùng tạm Camera.main nhưng có check lỗi
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (!iconVisual.activeSelf) return;
        if (playerCamera == null) return; // Nếu không có camera thì không xử lý tránh lỗi lay lắc

        // Billboard CHỈ hướng về duy nhất Player Camera, bỏ qua các camera phụ khác
        transform.LookAt(
            transform.position + playerCamera.transform.rotation * Vector3.forward,
            playerCamera.transform.rotation * Vector3.up
        );
    }

    public void Show() => iconVisual.SetActive(true);
    public void Hide() => iconVisual.SetActive(false);
}