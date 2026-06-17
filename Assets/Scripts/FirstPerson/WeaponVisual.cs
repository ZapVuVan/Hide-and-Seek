using System.Collections;
using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftArm;

    [Tooltip("Kéo object Aim vào đây")]
    [SerializeField] private Transform weaponRoot;


    [Tooltip("Kéo HoldPoint vào đây - nơi spawn prefab súng")]
    [SerializeField] private Transform holdPoint;  // HoldPoint - spawn model

    [SerializeField] private AudioSource audioSource;

    [Header("Left Arm Reload")]
    [SerializeField] private Vector3 leftArmDropOffset = new Vector3(0f, -0.25f, 0f);
    [SerializeField] private float leftArmLerpSpeed = 14f;

    [Header("Shoot Kick Effect")]
    [Tooltip("Độ giật dọc lên")]
    [SerializeField] private float kickUpAmount = 0.18f;

    [Tooltip("Độ lùi nhẹ")]
    [SerializeField] private float kickBackAmount = 0.05f;

    [Tooltip("Độ lắc ngang tối đa (random mỗi phát)")]
    [SerializeField] private float kickSideAmount = 0.06f;

    [Tooltip("Thời gian giật lên")]
    [SerializeField] private float kickUpTime = 0.02f;

    [Tooltip("Thời gian trở về")]
    [SerializeField] private float kickReturnTime = 0.07f;

    private GunData _currentGun;
    private GameObject _currentModel;

    private Vector3 _leftArmBasePos;
    private Vector3 _leftArmTarget;
    private Vector3 _weaponBasePos;

    private Coroutine _kickRoutine;

    private void Start()
    {
        if (leftArm != null)
            _leftArmBasePos = leftArm.localPosition;

        _leftArmTarget = _leftArmBasePos;

        if (weaponRoot != null)
            _weaponBasePos = weaponRoot.localPosition;

        ApplyEquippedGun();

        InventoryManager.OnInventoryChanged += ApplyEquippedGun;
    }

    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -= ApplyEquippedGun;
    }

    private void Update()
    {
        if (leftArm != null)
        {
            leftArm.localPosition = Vector3.Lerp(
                leftArm.localPosition,
                _leftArmTarget,
                Time.deltaTime * leftArmLerpSpeed
            );
        }
    }

    //─────────────────────────────
    // AUTO EQUIP
    //─────────────────────────────

    private void ApplyEquippedGun()
    {
        var gun = InventoryManager.Instance?.GetEquippedGun();
        if (gun != null)
            SetWeapon(gun);
    }

    //─────────────────────────────
    // PUBLIC API
    //─────────────────────────────

    public void SetWeapon(GunData gun)
    {
        if (_currentModel != null)
            Destroy(_currentModel);

        _currentGun = gun;

        if (gun == null || gun.weaponPrefab == null)
        {
            Debug.LogWarning("[WeaponVisual] Không có weaponPrefab!");
            return;
        }

        // ✅ Spawn vào HoldPoint thay vì weaponRoot
        _currentModel = Instantiate(gun.weaponPrefab, holdPoint);
    

        _weaponBasePos = weaponRoot.localPosition; // Aim vẫn dùng cho kick
    }

    public void PlayShootSound()
    {
        //if (_currentGun != null)
        //    PlaySound(_currentGun.shootSound);

        PlayShootKick();
    }

    public void PlayEmptySound()
    {
        //if (_currentGun != null)
        //    PlaySound(_currentGun.emptySound);
    }

    public void PlayReloadSequence(float reloadTime)
    {
        StartCoroutine(ReloadCoroutine(reloadTime));
    }

    //─────────────────────────────
    // SHOOT EFFECT
    //─────────────────────────────

    private void PlayShootKick()
    {
        if (weaponRoot == null)
            return;

        if (_kickRoutine != null)
            StopCoroutine(_kickRoutine);

        _kickRoutine = StartCoroutine(ShootKickCoroutine());
    }

    private IEnumerator ShootKickCoroutine()
    {
        Vector3 startPos = _weaponBasePos;

        float sideDir = (Random.value > 0.5f) ? 1f : -1f;
        float sideRand = Random.Range(0.3f, 1f);

        Vector3 kickPos = startPos + new Vector3(
            sideDir * kickSideAmount * sideRand,
            kickUpAmount,
            -kickBackAmount
        );

        float t = 0f;

        // Phase 1: Giật lên nhanh
        while (t < kickUpTime)
        {
            t += Time.deltaTime;
            weaponRoot.localPosition = Vector3.Lerp(
                startPos,
                kickPos,
                t / kickUpTime
            );
            yield return null;
        }

        t = 0f;

        // Phase 2: Trở về chậm hơn
        while (t < kickReturnTime)
        {
            t += Time.deltaTime;
            weaponRoot.localPosition = Vector3.Lerp(
                kickPos,
                startPos,
                t / kickReturnTime
            );
            yield return null;
        }

        weaponRoot.localPosition = startPos;
        _kickRoutine = null;
    }

    //─────────────────────────────
    // RELOAD VISUAL
    //─────────────────────────────

    private IEnumerator ReloadCoroutine(float time)
    {
        _leftArmTarget = _leftArmBasePos + leftArmDropOffset;

        yield return new WaitForSeconds(time * 0.2f);

        //if (_currentGun != null)
        //    PlaySound(_currentGun.reloadEjectSound);

        yield return new WaitForSeconds(time * 0.4f);

        //if (_currentGun != null)
        //    PlaySound(_currentGun.reloadInsertSound);

        yield return new WaitForSeconds(time * 0.3f);

        _leftArmTarget = _leftArmBasePos;
    }

    //─────────────────────────────
    // SOUND
    //─────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}