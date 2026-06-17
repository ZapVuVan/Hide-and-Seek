using UnityEngine;

/// <summary>
/// Gắn vào: FPS_Camera > Viewmodel_Rig
///
/// Tích hợp với:
///   - TouchCameraController  → lấy delta rotation của orientation để làm Sway
///   - PlayerMovement         → lấy speed/inputMove để làm Bobbing
///
/// KHÔNG có Recoil (theo yêu cầu).
/// </summary>
public class ViewmodelRig : MonoBehaviour
{
    // ──────────────────────────────────────────────────────
    // REFERENCES
    // ──────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Transform orientation (dùng chung với TouchCameraController)")]
    [SerializeField] private Transform orientation;

    [Tooltip("PlayerMovement để lấy tốc độ di chuyển")]
    [SerializeField] private PlayerMovement playerMovement;

    // ──────────────────────────────────────────────────────
    // WEAPON SWAY
    // ──────────────────────────────────────────────────────
    [Header("Weapon Sway – Lắc theo camera")]
    [Tooltip("Cường độ lắc theo hướng ngang (khi quay camera)")]
    [SerializeField] private float swayAmountX = 0.035f;

    [Tooltip("Cường độ lắc theo hướng dọc")]
    [SerializeField] private float swayAmountY = 0.020f;

    [Tooltip("Tốc độ kéo súng về tâm")]
    [SerializeField] private float swaySmoothing = 7f;

    [Tooltip("Giới hạn lệch tối đa")]
    [SerializeField] private float swayClamp = 0.10f;

    // ──────────────────────────────────────────────────────
    // WEAPON BOBBING
    // ──────────────────────────────────────────────────────
    [Header("Weapon Bobbing – Nhấp nhô bước chân")]
    [Tooltip("Tần số bước (bước/giây)")]
    [SerializeField] private float bobFrequency = 6f;

    [Tooltip("Biên độ lên xuống")]
    [SerializeField] private float bobAmplitudeY = 0.022f;

    [Tooltip("Biên độ ngang (tạo hình số 8, tần số × 0.5)")]
    [SerializeField] private float bobAmplitudeX = 0.010f;

    [Tooltip("Tốc độ blend vào/ra bobbing khi bắt đầu/dừng chạy")]
    [SerializeField] private float bobBlendSpeed = 8f;

    [Tooltip("Tốc độ tối thiểu để coi là 'đang chạy'")]
    [SerializeField] private float bobMinSpeed = 0.5f;

    // ──────────────────────────────────────────────────────
    // INTERNAL STATE
    // ──────────────────────────────────────────────────────
    // Vị trí gốc của Rig (tính ở Start, tất cả offset cộng lên đây)
    private Vector3 _originPos;

    // Sway
    private Vector3 _swayOffset;
    private Quaternion _prevOriRotation;

    // Bobbing
    private float _bobTimer;
    private float _bobWeight;   // 0 = đứng yên, 1 = đang chạy full
    private Vector3 _bobOffset;

    // ──────────────────────────────────────────────────────
    // INIT
    // ──────────────────────────────────────────────────────
    private void Start()
    {
        _originPos = transform.localPosition;

        if (orientation != null)
            _prevOriRotation = orientation.rotation;
    }

    // ──────────────────────────────────────────────────────
    // UPDATE
    // ──────────────────────────────────────────────────────
    private void Update()
    {
        //UpdateSway();
        UpdateBobbing();

        // Apply tổng offset
        transform.localPosition = _originPos + _swayOffset + _bobOffset;
    }

    // ──────────────────────────────────────────────────────
    // SWAY
    // Đọc delta rotation của orientation giữa 2 frame
    // (TouchCameraController thay đổi orientation.rotation mỗi frame)
    // ──────────────────────────────────────────────────────
    //private void UpdateSway()
    //{
    //    if (orientation == null) return;

    //    // Delta góc xoay so với frame trước
    //    Quaternion deltaRot = orientation.rotation * Quaternion.Inverse(_prevOriRotation);
    //    _prevOriRotation = orientation.rotation;

    //    // Chuyển sang Euler, chuẩn hoá về -180..180
    //    Vector3 euler = deltaRot.eulerAngles;
    //    if (euler.x > 180f) euler.x -= 360f;
    //    if (euler.y > 180f) euler.y -= 360f;

    //    // Lắc ngược hướng xoay (cảm giác nặng)
    //    Vector3 targetSway = new Vector3(
    //        -euler.y * swayAmountX,   // xoay ngang → lắc X
    //        -euler.x * swayAmountY,   // xoay dọc  → lắc Y
    //         0f
    //    );
    //    targetSway = Vector3.ClampMagnitude(targetSway, swayClamp);

    //    _swayOffset = Vector3.Lerp(_swayOffset, targetSway, Time.deltaTime * swaySmoothing);
    //}

    // ──────────────────────────────────────────────────────
    // BOBBING
    // Dùng PlayerMovement.GetSpeed() để biết đang chạy không
    // ──────────────────────────────────────────────────────
    private void UpdateBobbing()
    {
        float speed = playerMovement != null ? playerMovement.GetSpeed() : 0f;
        bool isMoving = speed > bobMinSpeed && !playerMovement.IsClimbing;

        // Blend weight mượt mà
        float targetWeight = isMoving ? 1f : 0f;
        _bobWeight = Mathf.Lerp(_bobWeight, targetWeight, Time.deltaTime * bobBlendSpeed);

        // Tiến timer theo tốc độ thực để bobbing nhanh hơn khi chạy nhanh hơn
        if (_bobWeight > 0.001f)
            _bobTimer += Time.deltaTime * bobFrequency;

        // Hình số 8: Y dùng Sin, X dùng Cos ở tần số 0.5
        float bobY = Mathf.Sin(_bobTimer) * bobAmplitudeY * _bobWeight;
        float bobX = Mathf.Cos(_bobTimer * 0.5f) * bobAmplitudeX * _bobWeight;

        // Lerp để vào ra mượt
        _bobOffset = Vector3.Lerp(_bobOffset, new Vector3(bobX, bobY, 0f),
                                  Time.deltaTime * bobBlendSpeed);
    }

    // ──────────────────────────────────────────────────────
    // PUBLIC: gọi từ WeaponController khi cần reset về gốc
    // (vd: khi swap súng muốn súng snap thẳng vào)
    // ──────────────────────────────────────────────────────
    public void SnapToOrigin()
    {
        _swayOffset = Vector3.zero;
        _bobOffset = Vector3.zero;
        transform.localPosition = _originPos;
    }
}