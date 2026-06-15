using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Tab Rarity Highlight")]
    public Image highlightAll;
    public Image highlightRare;
    public Image highlightEpic;
    public Image highlightLegendary;
    public Image highlightMythic;

    [Header("Grid")]
    public Transform gridContent;
    public GameObject itemSlotPrefab;

    [Header("Detail Panel")]
    public ItemDetailPanel detailPanel;

    [Header("Coins")]
    public TextMeshProUGUI coinsText;

    [Header("Panel Animation")]
    public float openDuration = 0.4f;
    public float closeDuration = 0.25f;
    public float openFromScale = 0.7f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    [Header("Slot Animation")]
    public float slotStagger = 0.04f;
    public float slotDuration = 0.3f;
    public Ease slotEase = Ease.OutBack;

    // ── private state ──
    private ItemType currentType = ItemType.Gun;
    private ItemRarity? rarityFilter = null;
    private ItemData selectedItem = null;

    private RectTransform panelRect;
    private CanvasGroup panelCG;
    private Tween panelTween;

    private List<ItemSlotUI> spawnedSlots = new();
    private List<Tween> slotTweens = new();

    // ─────────────────────────────────────────
    void Awake()
    {
        panelRect = inventoryPanel.GetComponent<RectTransform>();

        panelCG = inventoryPanel.GetComponent<CanvasGroup>();
        if (panelCG == null)
            panelCG = inventoryPanel.AddComponent<CanvasGroup>();
    }

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
        panelRect.localScale = Vector3.zero;
        panelCG.alpha = 1f;
    }

    // ─────────────────────────────────────────
    //  OPEN
    // ─────────────────────────────────────────
    public void Open()
    {
        currentType = ItemType.Gun;
        rarityFilter = null;
        selectedItem = null;

        RefreshCoins();
        UpdateRarityHighlight();
        Refresh(animate: true);
        detailPanel?.Hide();

        panelTween?.Kill();

        inventoryPanel.SetActive(true);
        panelRect.localScale = Vector3.one * openFromScale;
        panelCG.alpha = 1f;
        panelCG.interactable = true;
        panelCG.blocksRaycasts = true;

        panelTween = panelRect.DOScale(Vector3.one, openDuration)
                              .SetEase(openEase);
    }

    // ─────────────────────────────────────────
    //  CLOSE
    // ─────────────────────────────────────────
    public void Close()
    {
        panelTween?.Kill();

        panelCG.interactable = false;
        panelCG.blocksRaycasts = false;

        panelTween = DOTween.Sequence()
            .Append(panelRect.DOScale(Vector3.one * openFromScale, closeDuration)
                             .SetEase(closeEase))
            .OnComplete(() =>
            {
                inventoryPanel.SetActive(false);
                detailPanel?.Hide();
                selectedItem = null;
            });
    }

    // ─────────────────────────────────────────
    //  TAB TYPE
    // ─────────────────────────────────────────
    void SetType(ItemType type)
    {
        currentType = type;
        rarityFilter = null;
        selectedItem = null;
        detailPanel?.Hide();
        Refresh(animate: true);
    }

    // ─────────────────────────────────────────
    //  TAB RARITY
    // ─────────────────────────────────────────
    void SetRarity(ItemRarity? rarity)
    {
        rarityFilter = rarity;
        selectedItem = null;
        detailPanel?.Hide();
        UpdateRarityHighlight();
        Refresh(animate: true);
    }

    void UpdateRarityHighlight()
    {
        if (highlightAll) highlightAll.gameObject.SetActive(false);
        if (highlightRare) highlightRare.gameObject.SetActive(false);
        if (highlightEpic) highlightEpic.gameObject.SetActive(false);
        if (highlightLegendary) highlightLegendary.gameObject.SetActive(false);
        if (highlightMythic) highlightMythic.gameObject.SetActive(false);

        if (rarityFilter == null) highlightAll?.gameObject.SetActive(true);
        else if (rarityFilter == ItemRarity.Rare) highlightRare?.gameObject.SetActive(true);
        else if (rarityFilter == ItemRarity.Epic) highlightEpic?.gameObject.SetActive(true);
        else if (rarityFilter == ItemRarity.Legendary) highlightLegendary?.gameObject.SetActive(true);
        else if (rarityFilter == ItemRarity.Mythic) highlightMythic?.gameObject.SetActive(true);
    }

    // ─────────────────────────────────────────
    //  REFRESH
    // ─────────────────────────────────────────
    void Refresh(bool animate = false)
    {
        ClearSlots();
        SpawnSlots(animate);
    }

    void ClearSlots()
    {
        foreach (var t in slotTweens) t?.Kill();
        slotTweens.Clear();

        foreach (var s in spawnedSlots)
            if (s != null) Destroy(s.gameObject);
        spawnedSlots.Clear();
    }

    void SpawnSlots(bool animate)
    {
        if (InventoryManager.Instance == null) { Debug.LogError("[InventoryUI] InventoryManager NULL!"); return; }
        if (itemSlotPrefab == null) { Debug.LogError("[InventoryUI] itemSlotPrefab NULL!"); return; }
        if (gridContent == null) { Debug.LogError("[InventoryUI] gridContent NULL!"); return; }

        var items = InventoryManager.Instance.GetItemsByType(currentType);
        if (rarityFilter != null)
            items = items.FindAll(i => i.rarity == rarityFilter.Value);

        items.Sort((a, b) =>
        {
            bool aOwned = InventoryManager.Instance.IsOwned(a.itemId);
            bool bOwned = InventoryManager.Instance.IsOwned(b.itemId);
            if (aOwned != bOwned) return aOwned ? -1 : 1;
            return ((int)a.rarity).CompareTo((int)b.rarity);
        });

        string equippedId = InventoryManager.Instance.GetEquipped(currentType);

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null) continue;

            var go = Instantiate(itemSlotPrefab, gridContent);
            var slot = go.GetComponent<ItemSlotUI>();
            if (slot == null) continue;

            bool owned = InventoryManager.Instance.IsOwned(item.itemId);
            bool equipped = !string.IsNullOrEmpty(equippedId) && equippedId == item.itemId;
            int charges = InventoryManager.Instance.GetCharges(item.itemId);

            slot.Setup(item, owned, equipped, charges, OnSlotClicked);
            spawnedSlots.Add(slot);

            if (animate)
            {
                var rt = go.GetComponent<RectTransform>();
                rt.localScale = Vector3.zero;

                var t = rt.DOScale(Vector3.one, slotDuration)
                          .SetEase(slotEase)
                          .SetDelay(i * slotStagger);

                slotTweens.Add(t);
            }
        }

        if (selectedItem != null)
        {
            bool owned = InventoryManager.Instance.IsOwned(selectedItem.itemId);
            int charges = InventoryManager.Instance.GetCharges(selectedItem.itemId);
            detailPanel?.Show(selectedItem, owned, charges, OnBuyAction, OnEquipAction);
        }
    }

    // ─────────────────────────────────────────
    //  SLOT CLICKED
    // ─────────────────────────────────────────
    void OnSlotClicked(ItemData item)
    {
        selectedItem = item;
        bool owned = InventoryManager.Instance.IsOwned(item.itemId);
        int charges = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, owned, charges, OnBuyAction, OnEquipAction);
    }

    // ─────────────────────────────────────────
    //  BUY
    // ─────────────────────────────────────────
    void OnBuyAction(ItemData item)
    {
        var result = InventoryManager.Instance.TryBuy(item.itemId);
        if (result == InventoryManager.BuyResult.NotEnoughCoins)
        {
            Debug.Log("[Inventory] Không đủ coins!");
            panelRect.DOShakePosition(0.3f, new Vector3(8f, 0f, 0f), 15, 0)
                     .SetRelative(true);
            return;
        }

        RefreshCoins();
        selectedItem = null;
        Refresh(animate: false);

        selectedItem = item;
        bool ownedNow = InventoryManager.Instance.IsOwned(item.itemId);
        int chargesNow = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, ownedNow, chargesNow, OnBuyAction, OnEquipAction);
    }

    // ─────────────────────────────────────────
    //  EQUIP
    // ─────────────────────────────────────────
    void OnEquipAction(ItemData item)
    {
        InventoryManager.Instance.Equip(item.itemId);
        selectedItem = null;
        Refresh(animate: false);

        selectedItem = item;
        bool owned = InventoryManager.Instance.IsOwned(item.itemId);
        int charges = InventoryManager.Instance.GetCharges(item.itemId);
        detailPanel?.Show(item, owned, charges, OnBuyAction, OnEquipAction);
    }

    // ─────────────────────────────────────────
    //  COINS
    // ─────────────────────────────────────────
    void RefreshCoins()
    {
        if (coinsText != null)
            coinsText.text = InventoryManager.Instance.Coins.ToString();
    }

    // ─────────────────────────────────────────
    //  CLEANUP
    // ─────────────────────────────────────────
    void OnDestroy()
    {
        panelTween?.Kill();
        foreach (var t in slotTweens) t?.Kill();
        DOTween.Kill(panelRect);
    }
}