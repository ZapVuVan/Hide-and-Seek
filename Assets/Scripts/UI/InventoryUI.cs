using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel root")]
    public GameObject inventoryPanel;

    [Header("Tab Type")]
    public Button btnGun;
    public Button btnPower;
    public Button btnClose;

    [Header("Tab Rarity")]
    public Button btnAll;
    public Button btnRare;
    public Button btnEpic;
    public Button btnLegendary;
    public Button btnMythic;

    [Header("Grid")]
    public Transform gridContent;
    public GameObject itemSlotPrefab;

    [Header("Detail Panel")]
    public ItemDetailPanel detailPanel;

    [Header("Coins")]
    public TextMeshProUGUI coinsText;

    private ItemType currentType = ItemType.Gun;
    private ItemRarity? rarityFilter = null;
    private List<ItemSlotUI> spawnedSlots = new();
    private ItemData selectedItem = null;

    void Start()
    {
        btnGun?.onClick.AddListener(() => SetType(ItemType.Gun));
        btnPower?.onClick.AddListener(() => SetType(ItemType.Power));
        btnClose?.onClick.AddListener(Close);

        btnAll?.onClick.AddListener(() => SetRarity(null));
        btnRare?.onClick.AddListener(() => SetRarity(ItemRarity.Rare));
        btnEpic?.onClick.AddListener(() => SetRarity(ItemRarity.Epic));
        btnLegendary?.onClick.AddListener(() => SetRarity(ItemRarity.Legendary));
        btnMythic?.onClick.AddListener(() => SetRarity(ItemRarity.Mythic));

        inventoryPanel.SetActive(false);
    }

    public void Open()
    {
        inventoryPanel.SetActive(true);
        currentType = ItemType.Gun;
        rarityFilter = null;
        selectedItem = null;
        RefreshCoins();
        Refresh();
        detailPanel?.Hide();
    }

    public void Close()
    {
        inventoryPanel.SetActive(false);
        detailPanel?.Hide();
        selectedItem = null;
    }

    void SetType(ItemType type)
    {
        currentType = type;
        rarityFilter = null;
        selectedItem = null;
        detailPanel?.Hide();
        Refresh();
    }

    void SetRarity(ItemRarity? rarity)
    {
        rarityFilter = rarity;
        detailPanel?.Hide();
        selectedItem = null;
        Refresh();
    }

    void Refresh()
    {
        foreach (var s in spawnedSlots)
            if (s != null) Destroy(s.gameObject);
        spawnedSlots.Clear();

        if (InventoryManager.Instance == null) { Debug.LogError("[InventoryUI] InventoryManager NULL!"); return; }
        if (itemSlotPrefab == null) { Debug.LogError("[InventoryUI] itemSlotPrefab NULL!"); return; }
        if (gridContent == null) { Debug.LogError("[InventoryUI] gridContent NULL!"); return; }

        var items = InventoryManager.Instance.GetItemsByType(currentType);
        if (rarityFilter != null)
            items = items.FindAll(i => i.rarity == rarityFilter.Value);

        string equippedId = InventoryManager.Instance.GetEquipped(currentType);

        foreach (var item in items)
        {
            if (item == null) continue;
            var go = Instantiate(itemSlotPrefab, gridContent);
            var slot = go.GetComponent<ItemSlotUI>();
            if (slot == null) continue;

            bool owned = InventoryManager.Instance.IsOwned(item.itemId);
            bool equipped = !string.IsNullOrEmpty(equippedId) && equippedId == item.itemId;
            int charges = InventoryManager.Instance.GetCharges(item.itemId);

            slot.Setup(item, owned, equipped, charges, OnSlotClicked);
            spawnedSlots.Add(slot);
        }

        if (selectedItem != null)
        {
            bool owned = InventoryManager.Instance.IsOwned(selectedItem.itemId);
            int charges = InventoryManager.Instance.GetCharges(selectedItem.itemId);
            detailPanel?.Show(selectedItem, owned, charges, OnBuyAction, OnEquipAction);
        }
    }

    void OnSlotClicked(ItemData item)
    {
        selectedItem = item;
        bool owned = InventoryManager.Instance.IsOwned(item.itemId);
        int charges = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, owned, charges, OnBuyAction, OnEquipAction);
    }

    // Xử lý nút Mua
    void OnBuyAction(ItemData item)
    {
        var result = InventoryManager.Instance.TryBuy(item.itemId);
        if (result == InventoryManager.BuyResult.NotEnoughCoins)
        {
            Debug.Log("[Inventory] Không đủ coins!");
            return;
        }

        RefreshCoins();

        selectedItem = null;
        Refresh();

        // Sau khi mua → show lại detail, giờ owned=true → hiện nút Equip
        selectedItem = item;
        bool ownedNow = InventoryManager.Instance.IsOwned(item.itemId);
        int chargesNow = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, ownedNow, chargesNow, OnBuyAction, OnEquipAction);
    }

    // Xử lý nút Equip
    void OnEquipAction(ItemData item)
    {
        InventoryManager.Instance.Equip(item.itemId);

        selectedItem = null;
        Refresh();

        // Show lại detail sau equip
        selectedItem = item;
        bool owned = InventoryManager.Instance.IsOwned(item.itemId);
        int charges = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, owned, charges, OnBuyAction, OnEquipAction);
    }

    void RefreshCoins()
    {
        if (coinsText != null)
            coinsText.text = InventoryManager.Instance.Coins.ToString();
    }
}