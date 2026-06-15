using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder Instance { get; private set; }

    [Header("Weapon Holder Transform")]
    public Transform holdPoint; // Điểm đặt súng trong tay

    private GameObject currentWeapon;
    public GameObject CurrentWeapon => currentWeapon;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Tự động equip súng đang được trang bị khi vào game
        AutoEquipCurrentGun();
    }

    /// <summary>Đọc súng đang equip từ InventoryManager và hiển thị lên</summary>
    public void AutoEquipCurrentGun()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.database == null) return;

        string equippedId = InventoryManager.Instance.GetEquipped(ItemType.Gun);
        if (string.IsNullOrEmpty(equippedId)) return;

        var item = InventoryManager.Instance.database.GetById(equippedId);
        if (item is GunData gunData)
        {
            EquipWeapon(gunData);
            Debug.Log($"[WeaponHolder] Auto-equip khi start: {gunData.itemName}");
        }
    }

    public void EquipWeapon(GunData gunData)
    {
        // Xóa súng cũ
        if (currentWeapon != null)
            Destroy(currentWeapon);

        if (gunData == null || gunData.weaponPrefab == null)
        {
            Debug.LogWarning("[WeaponHolder] Không có weaponPrefab!");
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogError("[WeaponHolder] holdPoint chưa được gán!");
            return;
        }

        // Clone prefab vào holdPoint
        currentWeapon = Instantiate(gunData.weaponPrefab, holdPoint);
        // Giữ nguyên localPosition/Rotation/Scale đã set sẵn trong prefab

        Debug.Log($"[WeaponHolder] Đã equip: {gunData.itemName}");
    }

    public void ClearWeapon()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);
        currentWeapon = null;
    }
}