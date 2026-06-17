using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public static InventoryManager Instance { get; private set; }

    [Header("Data")]
    public ItemDatabase database;

    private const string OWNED_KEY = "owned_";
    private const string EQUIPPED_KEY = "equipped_";
    private const string CHARGES_KEY = "charges_";

    public int Coins => CoinManager.Instance != null ? CoinManager.Instance.GetCoin() : 0;

    public static System.Action OnInventoryChanged;
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitDefaultItems();
    }

    void InitDefaultItems()
    {
        if (database == null) return;
        foreach (var item in database.allItems)
        {
            if (item == null) continue;
            if (item.isDefault && !IsOwned(item.itemId))
                SetOwned(item.itemId, true);

            // FIX LỖI TÍCH: Nếu chưa có equipped nào cho type này → auto equip default
            if (item.isDefault && item.itemType != ItemType.Power)
            {
                string currentEquipped = GetEquipped(item.itemType);
                if (string.IsNullOrEmpty(currentEquipped))
                {
                    PlayerPrefs.SetString(EQUIPPED_KEY + item.itemType.ToString(), item.itemId);
                    PlayerPrefs.Save();
                }
            }
        }
    }

    public bool IsOwned(string itemId)
        => PlayerPrefs.GetInt(OWNED_KEY + itemId, 0) == 1;

    void SetOwned(string itemId, bool value)
    {
        PlayerPrefs.SetInt(OWNED_KEY + itemId, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public string GetEquipped(ItemType type)
        => PlayerPrefs.GetString(EQUIPPED_KEY + type.ToString(), "");

    public void Equip(string itemId)
    {
        var item = database.GetById(itemId);
        if (item == null) { Debug.LogError($"[Equip] Không tìm thấy item: {itemId}"); return; }
        if (!IsOwned(itemId)) { Debug.LogError($"[Equip] Chưa sở hữu: {itemId}"); return; }

        string key = EQUIPPED_KEY + item.itemType.ToString();
        PlayerPrefs.SetString(key, itemId);
        PlayerPrefs.Save();

        // ✅ Bỏ WeaponHolder, chỉ fire event — WeaponVisual tự lắng nghe
        OnInventoryChanged?.Invoke();

        Debug.Log($"[Equip] Đã equip: {itemId}");
    }

    public GunData GetEquippedGun()
    {
        string gunId = GetEquipped(ItemType.Gun);
        if (string.IsNullOrEmpty(gunId)) return null;
        return database.GetById(gunId) as GunData;
    }
    public int GetCharges(string itemId)
        => PlayerPrefs.GetInt(CHARGES_KEY + itemId, 0);

    void AddCharges(string itemId, int amount)
    {
        int current = GetCharges(itemId);
        PlayerPrefs.SetInt(CHARGES_KEY + itemId, current + amount);
        PlayerPrefs.Save();
    }

    public void UseCharge(string itemId)
    {
        int current = GetCharges(itemId);
        if (current <= 0) return;
        PlayerPrefs.SetInt(CHARGES_KEY + itemId, current - 1);
        PlayerPrefs.Save();
    }

    public enum BuyResult { Success, NotEnoughCoins, AlreadyOwned }

    public BuyResult TryBuy(string itemId)
    {
        var item = database.GetById(itemId);
        if (item == null) return BuyResult.NotEnoughCoins;

        if (item.itemType != ItemType.Power)
        {
            if (IsOwned(itemId)) return BuyResult.AlreadyOwned;
            if (!CoinManager.Instance.SpendCoin(item.price)) return BuyResult.NotEnoughCoins;
            SetOwned(itemId, true);

            // Phát sự kiện cập nhật inventory
            OnInventoryChanged?.Invoke();
            return BuyResult.Success;
        }

        // Power: +1 mỗi lần mua
        if (!CoinManager.Instance.SpendCoin(item.price)) return BuyResult.NotEnoughCoins;
        AddCharges(itemId, 1);

        // PHÁT SỰ KIỆN Ở ĐÂY để Hotbar nhận biết số lượng vừa tăng
        OnInventoryChanged?.Invoke();

        return BuyResult.Success;
    }

    public List<ItemData> GetAllItems() => database.GetAll();
    public List<ItemData> GetItemsByType(ItemType type) => database.GetByType(type);

    [ContextMenu("Reset All Charges")]
    void DEBUG_ResetAllCharges()
    {
        if (database == null) return;
        foreach (var item in database.allItems)
        {
            if (item != null && item.itemType == ItemType.Power)
            {
                PlayerPrefs.SetInt(CHARGES_KEY + item.itemId, 0);
                Debug.Log($"[DEBUG] Reset charges: {item.itemId}");
            }
        }
        PlayerPrefs.Save();
        Debug.Log("[DEBUG] Reset tất cả charges xong!");
    }

    [ContextMenu("Reset All Guns (Owned + Equipped)")]
    void DEBUG_ResetGuns()
    {
        if (database == null) return;

        // Xóa tất cả equipped keys trước
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (type != ItemType.Power)
                PlayerPrefs.DeleteKey(EQUIPPED_KEY + type.ToString());
        }

        foreach (var item in database.allItems)
        {
            if (item != null && item.itemType != ItemType.Power)
            {
                PlayerPrefs.DeleteKey(OWNED_KEY + item.itemId);
                Debug.Log($"[DEBUG] Reset gun: {item.itemId}");
            }
        }
        PlayerPrefs.Save();
        InitDefaultItems();
        Debug.Log("[DEBUG] Reset tất cả Gun xong!");
    }

    [ContextMenu("Reset ALL PlayerPrefs")]
    void DEBUG_ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        InitDefaultItems();
        Debug.Log("[DEBUG] Đã xóa toàn bộ PlayerPrefs!");
    }
}