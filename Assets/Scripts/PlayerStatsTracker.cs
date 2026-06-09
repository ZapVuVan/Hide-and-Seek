using UnityEngine;

[RequireComponent(typeof(RoleComponent))]
public class PlayerStatsTracker : MonoBehaviour
{
    public int KillCount { get; private set; }
    public float SurvivalTime { get; private set; }
    public float DistanceTraveled { get; private set; }
    public bool WasHider { get; private set; } // từng là hider lúc đầu game

    private RoleComponent role;
    private Vector3 lastPos;
    private bool trackingActive;

    private void Awake()
    {
        role = GetComponent<RoleComponent>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            // Bắt đầu track survival từ lúc HidingPhase (OnHidingPhaseStart)
            GameManager.Instance.OnHidingPhaseStart += HandleHidingPhaseStart;
            GameManager.Instance.OnGameFinish += HandleGameFinish;
        }

        var health = GetComponent<Health>();
        if (health != null)
            health.OnKilled += HandleKill;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHidingPhaseStart -= HandleHidingPhaseStart;
            GameManager.Instance.OnGameFinish -= HandleGameFinish;
        }

        var health = GetComponent<Health>();
        if (health != null)
            health.OnKilled -= HandleKill;
    }

    private void Update()
    {
        if (!trackingActive) return;

        // Distance — track tất cả mọi người
        Vector3 cur = transform.position;
        float dx = Vector3.Distance(cur, lastPos);
        if (dx > 0.01f)
        {
            DistanceTraveled += dx;
            lastPos = cur;
        }

        // SurvivalTime — chỉ tính khi còn là Hider và còn sống
        if (role.Role == GameRole.Hider && gameObject.activeSelf)
            SurvivalTime += Time.deltaTime;
    }

    private void HandleHidingPhaseStart(float duration, bool isSeeker)
    {
        // Reset stats mỗi game mới
        KillCount = 0;
        SurvivalTime = 0f;
        DistanceTraveled = 0f;
        lastPos = transform.position;
        trackingActive = true;

        // Ghi nhận ai là hider lúc đầu (role đã được assign trước HidingPhase)
        WasHider = role.Role == GameRole.Hider;
    }

    private void HandleGameFinish()
    {
        trackingActive = false;
    }

    private void HandleKill(GameObject killer, GameObject victim)
    {
        if (killer == gameObject && role.Role == GameRole.Seeker)
            KillCount++;
    }
}