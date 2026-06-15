using System;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, IRole, IFreezable
{
    [HideInInspector] public PlayerMovement movement;
    [SerializeField] private TouchCameraController touchCameraController;
    public GameObject headMeshRenderer;

    public GameObject hiderCamera;
    public GameObject seekerCamera;
    public GameObject hiderControlUI;
    public GameObject seekerControlUI;
    [Header("Shop")]
    public GameObject shopButton;

    private IPlayerState currentState;
    public PlayerNormalState normalState = new PlayerNormalState();
    public PlayerHiderState hiderState = new PlayerHiderState();
    public PlayerSeekerState seekerState = new PlayerSeekerState();

    // CHUYỂN GIAO: Trạng thái sống chết quản lý tập trung tại đây
    public bool IsDead { get; private set; } = false;
    private Health health;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<Health>();

        var killTable = FindObjectOfType<KillTable>();
        if (killTable != null && health != null)
            health.OnKilled += killTable.OnKilled;
    }

    private void Start()
    {
        GetComponent<RoleComponent>().SetRole(GameRole.None);

        // Đăng ký sự kiện từ Health để cập nhật trạng thái IsDead tập trung
        if (health != null)
        {
            health.OnDie += Handle_OnDie;
            health.OnRespawn += Handle_OnRespawn;
        }

        if (hiderCamera != null)
            hiderCamera.SetActive(true);

        if (seekerCamera != null)
            seekerCamera.SetActive(false);

        TransitionToState(normalState);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký để tránh memory leak
        if (health != null)
        {
            health.OnDie -= Handle_OnDie;
            health.OnRespawn -= Handle_OnRespawn;
        }
    }

    private void Handle_OnDie()
    {
        IsDead = true;
    }

    private void Handle_OnRespawn()
    {
        IsDead = false;
    }

    private void Update()
    {
        // Nếu đã chết, không cập nhật logic State Machine nữa
        if (IsDead) return;

        currentState?.UpdateState(this);
    }

    public void TransitionToState(IPlayerState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);

        bool isSeeker = currentState == seekerState;

        if (hiderCamera != null)
            hiderCamera.SetActive(!isSeeker);

        if (seekerCamera != null)
            seekerCamera.SetActive(isSeeker);

        if (isSeeker)
        {
            touchCameraController?.TransitionToFirstPerson();
        }
        else
        {
            touchCameraController?.TransitionToThirdPerson();
        }

        Debug.Log($"[PLAYER] State = {currentState.GetType().Name} | HiderCam = {(hiderCamera != null ? hiderCamera.activeSelf : false)}");
    }

    public bool IsFirstPerson() => currentState == seekerState;

    public void OnRoleChanged(GameRole role)
    {
        switch (role)
        {
            case GameRole.Hider:
                shopButton?.SetActive(true);
                hiderControlUI.SetActive(true);
                seekerControlUI.SetActive(false);
                TransitionToState(hiderState);
                WeaponHolder.Instance?.ClearWeapon();
                break;

            case GameRole.Seeker:
                shopButton?.SetActive(false);
                hiderControlUI.SetActive(false);
                seekerControlUI.SetActive(true);
                Debug.Log("Transitioning to Seeker State");
                TransitionToState(seekerState);
                LoadEquippedWeapon();
                break;

            default:
                shopButton?.SetActive(true);
                TransitionToState(normalState);
                WeaponHolder.Instance?.ClearWeapon();
                break;
        }
    }

    void LoadEquippedWeapon()
    {
        if (InventoryManager.Instance == null) return;

        string equippedId = InventoryManager.Instance.GetEquipped(ItemType.Gun);
        if (string.IsNullOrEmpty(equippedId)) return;

        var item = InventoryManager.Instance.database.GetById(equippedId);
        if (item is GunData gunData)
            WeaponHolder.Instance?.EquipWeapon(gunData);
    }

    public IPlayerState GetState() => currentState;

    public void ApplyFreeze(float duration)
    {
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        if (movement != null)
        {
            movement.SetFreeze(true);
        }

        yield return new WaitForSeconds(duration);

        if (movement != null)
        {
            movement.SetFreeze(false);
        }
    }
}