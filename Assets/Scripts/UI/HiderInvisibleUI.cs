using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm thư viện này để quản lý chữ TextMeshPro

public class HiderInvisibleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Kéo tấm ảnh màu xanh (ảnh dùng fillAmount) vào đây")]
    [SerializeField] private Image invisibleBar;

    // Cache các thành phần hiển thị để bật/tắt
    private Image[] _allImages;
    private TextMeshProUGUI[] _allTexts;

    private void Awake()
    {
        // Tự động lấy tất cả các Image và Text nằm trong Object này và các Object con
        _allImages = GetComponentsInChildren<Image>(true);
        _allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        // Nếu quên chưa kéo thanh fill vào Inspector, tự động tìm đứa con trùng tên
        if (invisibleBar == null)
        {
            foreach (Image img in _allImages)
            {
                // Tìm đứa con nào tên là InvisibleBar và có kiểu fill là Filled
                if (img.gameObject != this.gameObject && img.type == Image.Type.Filled)
                {
                    invisibleBar = img;
                    break;
                }
            }
        }
    }

    public void SetFill(float value)
    {
        if (invisibleBar != null)
        {
            invisibleBar.fillAmount = Mathf.Clamp01(value);
        }
    }

    public void SetBarVisible(bool visible)
    {
        if (_allImages != null)
        {
            for (int i = 0; i < _allImages.Length; i++)
            {
                if (_allImages[i] != null) _allImages[i].enabled = visible;
            }
        }

        // Bật/tắt tất cả chữ (Chữ "Tàng hình")
        if (_allTexts != null)
        {
            for (int i = 0; i < _allTexts.Length; i++)
            {
                if (_allTexts[i] != null) _allTexts[i].enabled = visible;
            }
        }
    }
}