// ==================== GameManager.cs ====================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private List<BotController> bots;
    [SerializeField] private GameWinUI gameWinUI;
    [SerializeField] private GameObject playerActionUI;

    [Header("AssignDelay")]
    [SerializeField] private float roleAssignDelay = 0.5f;
    [SerializeField] private RoleRevealUI roleRevealUI;
    [SerializeField] private float hidingPhaseDuration = 15f;
    [SerializeField] private float playingDuration = 90f;

    [Header("Ping Setting")]
    [SerializeField] private float pingInterval = 15f;
    [SerializeField] private float pingDuration = 5f;

    [Header("Coin Setting")]
    [SerializeField] private int killHiderCoin = 5;
    [SerializeField] private int surviveCoin = 1;
    [SerializeField] private float surviveInterval = 10f;

    [Header("Respawn Setting")]
    [SerializeField] private float timeRespawn = 5f;
    [SerializeField] private float deathAnimDelay = 2f;

    private GameState currentState;
    public GameState CurrentState => currentState;

    private Coroutine playingTimerCoroutine;
    private Coroutine pingCoroutine;
    private Coroutine hiderCoinCoroutine;

    private Health[] allHealth;

    private GameRole winnerRole; // ← THÊM MỚI

    // ================= EVENTS =================
    public event Action<float, bool> OnHidingPhaseStart;
    public event Action OnGameStart;
    public event Action<float> OnPlayingTimeUpdate;
    public event Action OnGameFinish;

    public event Action<float> OnPingCooldownUpdate;
    public event Action OnPingStart;
    public event Action OnPingEnd;
    public event Action<List<RoleComponent>> OnPingHiders;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        try
        {
            StartCoroutine(SpawnCharactersAtStartCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError("SPAWN ERROR: " + e);
        }

        if (RoleManager.Instance != null)
            RoleManager.Instance.OnRolesChanged += CheckGameEnd;

        TransitionToState(GameState.AssigningRoles);
    }

    private void OnEnable()
    {
        allHealth = FindObjectsOfType<Health>();

        foreach (var h in allHealth)
            h.OnKilled += HandleKill;
    }

    private void OnDisable()
    {
        if (allHealth == null) return;

        foreach (var h in allHealth)
            h.OnKilled -= HandleKill;
    }

    private void OnDestroy()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.OnRolesChanged -= CheckGameEnd;
    }

    // ================= SPAWN =================
    private IEnumerator SpawnCharactersAtStartCoroutine()
    {
        if (SpawnManager.Instance == null)
        {
            Debug.LogError("SpawnManager NULL");
            yield break;
        }

        if (player != null)
        {
            Transform playerSpawn = SpawnManager.Instance.GetRandomSpawnPoint();
            if (playerSpawn != null)
            {
                var cc    = player.GetComponent<CharacterController>();
                var rb    = player.GetComponent<Rigidbody>();
                var agent = player.GetComponent<NavMeshAgent>();

                if (cc != null)    cc.enabled = false;
                if (agent != null) agent.enabled = false;
                if (rb != null)    rb.isKinematic = true;

                player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);

                yield return null;

                if (cc != null)    cc.enabled = true;
                if (rb != null)    rb.isKinematic = false;
                if (agent != null)
                {
                    agent.enabled = true;
                    if (agent.isOnNavMesh) agent.Warp(playerSpawn.position);
                }
            }
        }

        foreach (var bot in bots)
        {
            if (bot == null) continue;
            Transform spawn = SpawnManager.Instance.GetRandomSpawnPoint();
            if (spawn == null) continue;

            var agent = bot.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            bot.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            yield return null;

            if (agent != null)
            {
                agent.enabled = true;
                if (agent.isOnNavMesh) agent.Warp(spawn.position);
            }
        }
    }

    // ================= STATE =================
    public void TransitionToState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.AssigningRoles:
                StartCoroutine(AssigningRolesCoroutine());
                break;

            case GameState.HidingPhase:
                StartCoroutine(HidingPhaseCoroutine());
                break;

            case GameState.Playing:
                OnPlaying();
                break;

            case GameState.GameEnd:
                OnGameEnd();
                break;
        }
    }

    // ================= ROLE =================
    private IEnumerator AssigningRolesCoroutine()
    {
        playerActionUI.SetActive(false);

        yield return new WaitForSeconds(roleAssignDelay);

        GameRole playerRole = RoleManager.Instance.GenerateRoles(player, bots);

        if (roleRevealUI != null)
            yield return roleRevealUI.PlayReveal(playerRole);

        RoleManager.Instance.ApplyRoles();

        if (player != null)
        {
            var playerRoleComp = player.GetComponent<RoleComponent>();

            bool isHider =
                playerRoleComp != null &&
                playerRoleComp.Role == GameRole.Hider;

            playerActionUI.SetActive(isHider);
        }

        TransitionToState(GameState.HidingPhase);
    }

    // ================= HIDING =================
    private IEnumerator HidingPhaseCoroutine()
    {
        bool isSeeker = player.GetComponent<RoleComponent>().Role == GameRole.Seeker;

        OnHidingPhaseStart?.Invoke(hidingPhaseDuration, isSeeker);

        yield return new WaitForSeconds(hidingPhaseDuration);

        TransitionToState(GameState.Playing);
    }

    // ================= PLAYING =================
    private void OnPlaying()
    {
        playerActionUI?.SetActive(true);
        playingTimerCoroutine = StartCoroutine(PlayingTimerCoroutine());
        pingCoroutine         = StartCoroutine(PingUI());
        hiderCoinCoroutine    = StartCoroutine(HiderSurviveCoin());
    }

    private IEnumerator PlayingTimerCoroutine()
    {
        float timeLeft = playingDuration;

        OnGameStart?.Invoke();

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            OnPlayingTimeUpdate?.Invoke(timeLeft);
            yield return null;
        }

        winnerRole = GameRole.Hider; // ← hết giờ, Hider thắng
        TransitionToState(GameState.GameEnd);
    }

    // ================= HIDER COIN =================
    private IEnumerator HiderSurviveCoin()
    {
        var role = player.GetComponent<RoleComponent>();

        if (role == null || role.Role != GameRole.Hider)
            yield break;

        while (currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(surviveInterval);

            if (currentState != GameState.Playing) yield break;
            if (role.Role != GameRole.Hider) yield break;

            CoinManager.Instance.AddCoin(surviveCoin);
            gameWinUI.AddMatchCoin(surviveCoin);
            NotificationCoin.Instance.ShowCoin(surviveCoin, 1);
        }
    }

    // ================= PING =================
    private IEnumerator PingUI()
    {
        while (true)
        {
            float t = 0f;

            while (t < pingInterval)
            {
                if (currentState != GameState.Playing) yield break;

                t += Time.deltaTime;
                OnPingCooldownUpdate?.Invoke((t / pingInterval));
                yield return null;
            }

            if (currentState != GameState.Playing) yield break;

            List<RoleComponent> hiders =
                RoleManager.Instance.GetAllByRole(GameRole.Hider);

            OnPingStart?.Invoke();
            OnPingHiders?.Invoke(hiders);

            HiderPingUIManager.Instance.SetHiders(hiders);

            yield return new WaitForSeconds(pingDuration);

            HiderPingUIManager.Instance.Clear();
            OnPingEnd?.Invoke();
        }
    }

    // ================= KILL =================
    private void HandleKill(GameObject killer, GameObject victim)
    {
        if (killer == null || victim == null) return;

        var killerRole = killer.GetComponent<RoleComponent>();
        var victimRole = victim.GetComponent<RoleComponent>();

        if (killerRole == null || victimRole == null) return;

        if (killerRole.Role == GameRole.Seeker &&
            victimRole.Role == GameRole.Hider)
        {
            if (killer.CompareTag("Player"))
            {
                CoinManager.Instance.AddCoin(killHiderCoin);
                gameWinUI.AddMatchCoin(killHiderCoin);
                NotificationCoin.Instance.ShowCoin(killHiderCoin, 2);
            }

            List<RoleComponent> currentHiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);

            if (currentHiders.Count <= 1 && currentHiders.Contains(victimRole))
            {
                if (victim.CompareTag("Player"))
                {
                    winnerRole = GameRole.Seeker; // ← THÊM MỚI
                    StartCoroutine(DelayedGameEnd(deathAnimDelay));
                }
                else
                {
                    winnerRole = GameRole.Seeker; // ← THÊM MỚI
                    victimRole.SetRole(GameRole.Seeker);
                    TransitionToState(GameState.GameEnd);
                }
                return;
            }

            StartCoroutine(RespawnAsSeeker(victim));
        }
    }

    // ================= DELAYED GAME END =================
    private IEnumerator DelayedGameEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        TransitionToState(GameState.GameEnd);
    }

    // ================= RESPAWN =================
    private IEnumerator RespawnAsSeeker(GameObject victim)
    {
        yield return new WaitForSeconds(timeRespawn);

        if (victim == null) yield break;
        if (currentState == GameState.GameEnd) yield break;

        var role        = victim.GetComponent<RoleComponent>();
        var agent       = victim.GetComponent<NavMeshAgent>();
        var healthComp  = victim.GetComponent<Health>();

        if (role == null) yield break;
        if (role.Role != GameRole.Hider) yield break;

        Transform spawn = SpawnManager.Instance.GetRandomSpawnPoint();
        if (spawn == null) yield break;

        if (agent != null)
            agent.enabled = false;

        victim.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        yield return null;

        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh) agent.Warp(spawn.position);
        }

        if (!victim.activeSelf)
            victim.SetActive(true);

        role.SetRole(GameRole.Seeker);

        if (healthComp != null)
            healthComp.RespawnHealth();

        CheckGameEnd();
    }

    // ================= GAME END =================
    private void OnGameEnd()
    {
        playerActionUI.SetActive(false);

        if (playingTimerCoroutine != null) StopCoroutine(playingTimerCoroutine);
        if (pingCoroutine != null)         StopCoroutine(pingCoroutine);
        if (hiderCoinCoroutine != null)    StopCoroutine(hiderCoinCoroutine);

        HiderPingUIManager.Instance.Clear();

        if (player != null)
        {
            var playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.SetFreeze(true);
        }

        if (bots != null)
        {
            foreach (var bot in bots)
            {
                if (bot == null) continue;

                var agent = bot.GetComponent<NavMeshAgent>();
                if (agent != null && agent.gameObject.activeSelf && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity  = Vector3.zero;
                }
            }
        }

        OnGameFinish?.Invoke();
        gameWinUI.Show(winnerRole); // ← SỬA: truyền winnerRole
    }

    public void CheckGameEnd()
    {
        if (currentState != GameState.Playing) return;

        if (RoleManager.Instance.CountByRole(GameRole.Hider) <= 0)
            TransitionToState(GameState.GameEnd);
    }
}