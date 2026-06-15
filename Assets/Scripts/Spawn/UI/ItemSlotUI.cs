using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image iconImage;
    public GameObject lockOverlay;
    public GameObject equippedOverlay;
    public TextMeshProUGUI chargeText;
    public Button slotButton;
    public RarityBackgroundConfig rarityConfig;

    private ItemData itemData;
    private Action<ItemData> onClickCallback;

    public void Setup(ItemData data, bool owned, bool equipped, int charges, Action<ItemData> onClick)
    {
        itemData = data;
        onClickCallback = onClick;

        if (iconImage != null) iconImage.sprite = data.icon;

        if (backgroundImage != null && rarityConfig != null)
        {
            var sprite = rarityConfig.GetSprite(data.rarity);
            if (sprite != null) backgroundImage.sprite = sprite;
        }

        if (data.itemType == ItemType.Power)
        {
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
            lockOverlay?.SetActive(!owned);
            equippedOverlay?.SetActive(equipped);
            if (chargeText != null)
                chargeText.gameObject.SetActive(false);
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => onClickCallback?.Invoke(itemData));
    }
}