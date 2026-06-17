using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ShootButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private TouchCameraController touchCamera;
    [SerializeField] private WeaponVisual weaponVisual;
    [Header("Reload Button")]
    [SerializeField] private Button reloadButton;
    private GunData currentGun;
    private int currentAmmo;
    private bool isReloading = false;
    private bool isShooting = false;
    private float nextFireTime = 0f;
    private void Start()
    {
        LoadEquippedGun();
        InventoryManager.OnInventoryChanged += LoadEquippedGun;
        if (reloadButton != null)
            reloadButton.onClick.AddListener(OnClickReload);
    }
    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -= LoadEquippedGun;
        if (reloadButton != null)
            reloadButton.onClick.RemoveListener(OnClickReload);
    }
    private void LoadEquippedGun()
    {
        if (InventoryManager.Instance == null) return;
        string id = InventoryManager.Instance.GetEquipped(ItemType.Gun);
        if (string.IsNullOrEmpty(id)) return;
        var item = InventoryManager.Instance.database.GetById(id);
        if (item is GunData gun)
        {
            currentGun = gun;
            currentAmmo = gun.magazineSize;
            isReloading = false;
            MagazineGunUI.Instance?.UpdateGun(gun);
            MagazineGunUI.Instance?.UpdateAmmo(currentAmmo, currentGun.magazineSize);

            weaponVisual?.SetWeapon(gun); // <-- đổi model/âm thanh theo súng đang trang bị
        }
    }
    private void Update()
    {
        if (!isShooting || currentGun == null || isReloading) return;
        if (!currentGun.isAutomatic) return;
        if (Time.time < nextFireTime) return;
        TryShoot();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentGun == null || isReloading) return;
        isShooting = true;
        nextFireTime = 0f;
        TryShoot();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isShooting = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        touchCamera.OnDrag(eventData);
    }
    // Gắn vào Button Reload trong Inspector hoặc tự gắn qua reloadButton
    public void OnClickReload()
    {
        if (currentGun == null || isReloading) return;
        if (currentAmmo == currentGun.magazineSize) return; // đã đầy đạn
        StartCoroutine(Reload());
    }
    private void TryShoot()
    {
        if (currentAmmo <= 0)
        {
            weaponVisual?.PlayEmptySound(); // <-- bắn mà hết đạn: phát tiếng "tách" rỗng
            StartCoroutine(Reload());
            return;
        }
        playerShoot.Shoot();
        currentAmmo--;
        nextFireTime = Time.time + (1f / currentGun.fireRate);
        MagazineGunUI.Instance?.UpdateAmmo(currentAmmo, currentGun.magazineSize);

        weaponVisual?.PlayShootSound(); // <-- mỗi lần bắn thành công

        if (currentAmmo <= 0) StartCoroutine(Reload());
    }
    private System.Collections.IEnumerator Reload()
    {
        if (isReloading || currentGun == null) yield break;
        isReloading = true;
        isShooting = false;
        if (reloadButton != null) reloadButton.interactable = false;
        MagazineGunUI.Instance?.ShowReloading();

        weaponVisual?.PlayReloadSequence(currentGun.reloadTime); // <-- chạy song song hiệu ứng tay/âm thanh reload

        yield return new WaitForSeconds(currentGun.reloadTime);
        currentAmmo = currentGun.magazineSize;
        isReloading = false;
        if (reloadButton != null) reloadButton.interactable = true;
        MagazineGunUI.Instance?.UpdateAmmo(currentAmmo, currentGun.magazineSize);
    }
}