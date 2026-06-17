using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagazineGunUI : MonoBehaviour
{
    public static MagazineGunUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image gunIcon; // Image ô vuông trong UI

    private int cachedMax = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void UpdateGun(GunData gun)
    {
        cachedMax = gun.magazineSize;

        // Hiện icon súng
        if (gunIcon != null && gun.icon != null)
        {
            gunIcon.sprite = gun.icon;
            gunIcon.enabled = true;
        }
    }

    public void UpdateAmmo(int current, int max)
    {
        cachedMax = max;
        if (ammoText == null) return;
        // Format 2 chữ số: 1 → 01, 12 → 12
        ammoText.text = $"{current:D2} / {max:D2}";
    }

    public void ShowReloading()
    {
        if (ammoText == null) return;
        ammoText.text = $"__/{cachedMax:D2}";
    }
}