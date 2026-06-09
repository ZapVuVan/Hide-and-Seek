using UnityEngine;
using UnityEngine.UI;

public class HiderInvisibleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image invisibleBar;

    private void Start()
    {
        // Nếu chưa kéo thả trong Inspector, tự tìm Image trên chính Object này
        if (invisibleBar == null)
        {
            invisibleBar = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        InvisibleController.OnInvisibleUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        InvisibleController.OnInvisibleUpdated -= UpdateUI;
    }

    // Xử lý trực tiếp: Vừa tăng giảm fillAmount, vừa ẩn hiện ảnh nếu cần
    private void UpdateUI(float fillValue)
    {
        if (invisibleBar == null) return;

        // Cập nhật giá trị thanh bar
        invisibleBar.fillAmount = Mathf.Clamp01(fillValue);

        // Tự động ẩn hẳn Image đi khi giá trị bằng 0 (nếu bạn muốn ẩn khi không tàng hình)
        // Hoặc bạn có thể xóa 2 dòng dưới nếu muốn thanh bar luôn hiển thị trên màn hình.
        bool shouldShow = fillValue > 0f;
        if (invisibleBar.enabled != shouldShow) invisibleBar.enabled = shouldShow;
    }

    // Các hàm Force cũ nếu bạn còn gọi từ nơi khác (giữ lại để không lỗi code)
    public void ForceShow() { if (invisibleBar != null) invisibleBar.enabled = true; }
    public void ForceHide() { if (invisibleBar != null) invisibleBar.enabled = false; }
    public void SetFill(float value) { if (invisibleBar != null) invisibleBar.fillAmount = Mathf.Clamp01(value); }
    public void SetBarVisible(bool visible) { if (invisibleBar != null) invisibleBar.enabled = visible; }
}