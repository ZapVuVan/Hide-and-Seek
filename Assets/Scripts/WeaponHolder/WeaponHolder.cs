using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public static WeaponHolder Instance { get; private set; }

    [Header("Weapon Holder Transform")]
    public Transform holdPoint; // Điểm đặt súng trong tay

    private GameObject currentWeapon;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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

        // Spawn súng mới vào holdPoint
        currentWeapon = Instantiate(gunData.weaponPrefab, holdPoint);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = Vector3.one;

        Debug.Log($"[WeaponHolder] Đã equip: {gunData.itemName}");
    }

    public void ClearWeapon()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);
        currentWeapon = null;
    }
}