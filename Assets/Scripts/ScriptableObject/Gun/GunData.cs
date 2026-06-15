using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "Inventory/Gun")]
public class GunData : ItemData
{
    [Header("Gun Stats")]
    public float damage = 10f;
    public int magazineSize = 30;
    public float reloadTime = 1.5f;
    public float fireRate = 10f;        // Số phát/giây
    public bool isAutomatic = false;    // Giữ để sấy hay chỉ bắn từng phát

    [Header("Prefab")]
    public GameObject weaponPrefab;

    void OnValidate() => itemType = ItemType.Gun;
}