using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Image borderImage;
    public GameObject lockOverlay;      // chỉ dùng cho Gun
    public GameObject equippedOverlay;  // chỉ dùng cho Gun
    public TextMeshProUGUI chargeText;  // chỉ dùng cho Power: "x2"
    public Button slotButton;

    private ItemData itemData;
    private Action<ItemData> onClickCallback;

    public void Setup(ItemData data, bool owned, bool equipped, int charges, Action<ItemData> onClick)
    {
        itemData = data;
        onClickCallback = onClick;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (borderImage != null) borderImage.color = data.GetRarityColor();

        if (data.itemType == ItemType.Power)
        {
            // Power: không khóa, hiện số lượng kể cả khi = 0
            lockOverlay?.SetActive(false);
            equippedOverlay?.SetActive(false);
            if (chargeText != null)
            {
                chargeText.gameObject.SetActive(true);
                chargeText.text = $"x{charges}";
            }
        }
        else
        {
            // FIX LỖI 1: Đảm bảo chargeText bị ẩn hoàn toàn với Gun/Knife/Emote
            lockOverlay?.SetActive(!owned);
            equippedOverlay?.SetActive(equipped);
            if (chargeText != null)
                chargeText.gameObject.SetActive(false);
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => onClickCallback?.Invoke(itemData));
    }
}