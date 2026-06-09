using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "Inventory/Gun")]
public class GunData : ItemData
{
    [Header("Gun Stats")]
    public float damage = 10f;
    public float fireRate = 5f;
    public float bulletSpeed = 20f;
    public int magazineSize = 30;
    public float reloadTime = 1.5f;
    public bool isAutomatic = false;

    [Header("Prefab")]
    public GameObject weaponPrefab;

    void OnValidate() => itemType = ItemType.Gun;
}