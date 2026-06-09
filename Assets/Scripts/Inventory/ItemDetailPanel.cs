using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public GameObject buyButton;
    public GameObject equipButton;      // Thêm nút Equip riêng trong Unity
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI chargesOwnedText;

    private ItemData currentItem;
    private Action<ItemData> onBuy;
    private Action<ItemData> onEquip;

    void Start() => gameObject.SetActive(false);

    public void Show(ItemData item, bool owned, int charges,
                     Action<ItemData> onBuyAction, Action<ItemData> onEquipAction)
    {
        currentItem = item;
        onBuy = onBuyAction;
        onEquip = onEquipAction;
        gameObject.SetActive(true);

        if (itemIcon != null) itemIcon.sprite = item.icon;
        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemDescText != null) itemDescText.text = item.description;
        if (priceText != null) priceText.text = item.price.ToString();

        if (item.itemType == ItemType.Power)
        {
            // Power: chỉ có nút mua, không equip
            buyButton?.SetActive(true);
            equipButton?.SetActive(false);
            if (chargesOwnedText != null)
            {
                chargesOwnedText.gameObject.SetActive(true);
                chargesOwnedText.text = charges > 0 ? $"Bạn có: x{charges}" : "Chưa có";
            }
        }
        else
        {
            // Gun
            chargesOwnedText?.gameObject.SetActive(false);

            if (!owned)
            {
                // Chưa mua → chỉ hiện nút Mua
                buyButton?.SetActive(true);
                equipButton?.SetActive(false);
            }
            else
            {
                // Đã owned → ẩn nút Mua, hiện nút Equip
                buyButton?.SetActive(false);
                equipButton?.SetActive(true);
            }
        }

        // Gán action Mua
        var buyBtn = buyButton?.GetComponent<Button>();
        buyBtn?.onClick.RemoveAllListeners();
        buyBtn?.onClick.AddListener(() => onBuy?.Invoke(currentItem));

        // Gán action Equip
        var eqBtn = equipButton?.GetComponent<Button>();
        eqBtn?.onClick.RemoveAllListeners();
        eqBtn?.onClick.AddListener(() => onEquip?.Invoke(currentItem));
    }

    public void Hide() => gameObject.SetActive(false);
}