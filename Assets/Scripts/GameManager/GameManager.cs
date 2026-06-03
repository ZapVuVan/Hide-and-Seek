using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private List<BotController> bots;
    //[SerializeField] private RoleUI roleUI;
    [SerializeField] private HidingPhaseUI hidingPhaseUI;
    [SerializeField] private TimePlayGameUI timePlayGameUI;
    [SerializeField] private GameWinUI gameWinUI;
    [SerializeField] private GameObject playerActionUI;
    [SerializeField] private RoleRevealUI roleRevealUI;
    [SerializeField] private TimeHiderUI timeHiderUI;

    [Header("Ping UI")]
    [SerializeField] private HiderPingUIManager hiderPingUIManager;

    [Header("Config")]
    [SerializeField] private float roleAssignDelay = 3f;
    [SerializeField] private float hidingPhaseDuration = 15f;
    [SerializeField] private float playingDuration = 90f;

    [Header("Ping Config")]
    [SerializeField] private float pingInterval = 15f;
    [SerializeField] private float pingDuration = 5f;

    [Header("Coin Config")]
    [SerializeField] private int killHiderCoin = 5;
    [SerializeField] private int surviveCoin = 1;
    [SerializeField] private float surviveInterval = 10f;

    private GameState currentState;
    public GameState CurrentState => currentState;

    private Coroutine playingTimerCoroutine;
    private Coroutine pingCoroutine;
    private Coroutine hiderCoinCoroutine;

    private Health[] allHealth;

    // ================= EVENTS =================
    public event Action<float> OnPingCooldownUpdate;
    public event Action OnPingStart;
    public event Action OnPingEnd;
    public event Action<List<RoleComponent>> OnPingHiders;

    private void Awake()
    {
        Instance = this;
    }

    // ================= SUBSCRIBE KILL EVENT =================
    private void OnEnable()
    {
        allHealth = FindObjectsOfType<Health>();

        foreach (var h in allHealth)
        {
            h.OnKilled += HandleKill;
        }
    }

    private void OnDisable()
    {
        if (allHealth == null) return;

        foreach (var h in allHealth)
        {
            h.OnKilled -= HandleKill;
        }
    }

    private void Start()
    {
        RoleManager.Instance.OnRolesChanged += CheckGameEnd;
        TransitionToState(GameState.AssigningRoles);
    }

    private void OnDestroy()
    {
        RoleManager.Instance.OnRolesChanged -= CheckGameEnd;
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

    // ================= ROLE ASSIGN =================
    private IEnumerator AssigningRolesCoroutine()
    {
        playerActionUI.SetActive(false);

        yield return new WaitForSeconds(roleAssignDelay);

        List<GameObject> allCharacters = new List<GameObject>();
        allCharacters.Add(player.gameObject);

        foreach (var bot in bots)
            allCharacters.Add(bot.gameObject);

        int seekerIndex = UnityEngine.Random.Range(0, allCharacters.Count);

        GameRole playerRole = GameRole.Hider;

        for (int i = 0; i < allCharacters.Count; i++)
        {
            GameRole role = (i == seekerIndex) ? GameRole.Seeker : GameRole.Hider;
            allCharacters[i].GetComponent<RoleComponent>()?.SetRole(role);

            if (i == 0)
                playerRole = role;
        }

        if (roleRevealUI != null)
            yield return StartCoroutine(roleRevealUI.PlayReveal(playerRole));

        playerActionUI.SetActive(true);
        TransitionToState(GameState.HidingPhase);
    }

    // ================= HIDING =================
    private IEnumerator HidingPhaseCoroutine()
    {
        bool playerIsSeeker = player.GetComponent<RoleComponent>().Role == GameRole.Seeker;

        if (playerIsSeeker)
            hidingPhaseUI.Show();
        else
            timeHiderUI.Show();

        float timeLeft = hidingPhaseDuration;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            if (playerIsSeeker)
                hidingPhaseUI.UpdateTimer(timeLeft);
            else
                timeHiderUI.UpdateTimer(timeLeft);

            yield return null;
        }

        hidingPhaseUI.Hide();
        timeHiderUI.Hide();

        TransitionToState(GameState.Playing);
    }

    // ================= PLAYING =================
    private void OnPlaying()
    {
        playerActionUI.SetActive(true);
        //roleUI.Show();

        playingTimerCoroutine = StartCoroutine(PlayingTimerCoroutine());
        pingCoroutine = StartCoroutine(PingUI());

        hiderCoinCoroutine = StartCoroutine(HiderSurviveCoin());
    }

    private IEnumerator PlayingTimerCoroutine()
    {
        float timeLeft = playingDuration;

        timePlayGameUI.Show();

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            timePlayGameUI.UpdateTimer(timeLeft);
            yield return null;
        }

        timePlayGameUI.Hide();
        TransitionToState(GameState.GameEnd);
    }

    // ================= HIDER SURVIVE COIN =================
    private IEnumerator HiderSurviveCoin()
    {
        var role = player.GetComponent<RoleComponent>();

        if (role.Role != GameRole.Hider)
            yield break;

        while (currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(surviveInterval);

            if (currentState != GameState.Playing) yield break;
            if (role.Role != GameRole.Hider) yield break;

            CoinManager.Instance.AddCoin(surviveCoin);
            NotificationCoin.Instance.ShowCoin(surviveCoin,1);
        }
    }

    // ================= PING SYSTEM =================
    private IEnumerator PingUI()
    {
        while (true)
        {
            float t = 0f;

            while (t < pingInterval)
            {
                t += Time.deltaTime;
                OnPingCooldownUpdate?.Invoke(1f - (t / pingInterval));
                yield return null;
            }

            List<RoleComponent> hiders =
                RoleManager.Instance.GetAllByRole(GameRole.Hider);

            OnPingStart?.Invoke();
            OnPingHiders?.Invoke(hiders);

            hiderPingUIManager.SetHiders(hiders);

            yield return new WaitForSeconds(pingDuration);

            hiderPingUIManager.Clear();

            OnPingEnd?.Invoke();
        }
    }

    // ================= KILL REWARD =================
    private void HandleKill(GameObject killer, GameObject victim)
    {
        if (killer == null || victim == null) return;

        var killerRole = killer.GetComponent<RoleComponent>();
        var victimRole = victim.GetComponent<RoleComponent>();

        if (killerRole == null || victimRole == null) return;

        // 🔴 SEEKER giết HIDER
        if (killerRole.Role == GameRole.Seeker &&
            victimRole.Role == GameRole.Hider && killer.tag == "Player")
        {
            CoinManager.Instance.AddCoin(killHiderCoin);
            NotificationCoin.Instance.ShowCoin(killHiderCoin,2);
        }
    }

    // ================= GAME END =================
    private void OnGameEnd()
    {
        playerActionUI.SetActive(false);

        if (playingTimerCoroutine != null)
            StopCoroutine(playingTimerCoroutine);

        if (pingCoroutine != null)
            StopCoroutine(pingCoroutine);

        if (hiderCoinCoroutine != null)
            StopCoroutine(hiderCoinCoroutine);

        hiderPingUIManager.Clear();

        timePlayGameUI.Hide();
        gameWinUI.Show();
    }

    public void CheckGameEnd()
    {
        if (currentState != GameState.Playing) return;

        int hiderCount = RoleManager.Instance.CountByRole(GameRole.Hider);

        if (hiderCount <= 0)
            TransitionToState(GameState.GameEnd);
    }
}