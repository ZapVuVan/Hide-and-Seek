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
    [SerializeField] private RoleUI roleUI;
    [SerializeField] private HidingPhaseUI hidingPhaseUI;
    [SerializeField] private TimePlayGameUI timePlayGameUI;
    [SerializeField] private GameWinUI gameWinUI;
    [SerializeField] private GameObject playerActionUI;
    [SerializeField] private RoleRevealUI roleRevealUI;
    [SerializeField] private TimeHiderUI timeHiderUI;

    [Header("Config")]
    [SerializeField] private float roleAssignDelay = 3f;
    [SerializeField] private float hidingPhaseDuration = 15f;
    [SerializeField] private float playingDuration = 90f;

    [Header("Ping Config")]
    [SerializeField] private float pingInterval = 15f;
    [SerializeField] private float pingDuration = 5f;

    private GameState currentState;
    public GameState CurrentState => currentState;

    private Coroutine playingTimerCoroutine;
    private Coroutine pingCoroutine;

    // ================= EVENTS =================
    public event Action<float> OnPingCooldownUpdate;
    public event Action OnPingStart;
    public event Action OnPingEnd;
    public event Action<List<RoleComponent>> OnPingHiders;

    private void Awake() => Instance = this;

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

    // ================= ROLE =================
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

        LockSeekers(true);

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

        LockSeekers(false);

        hidingPhaseUI.Hide();
        timeHiderUI.Hide();

        TransitionToState(GameState.Playing);
    }

    // ================= PLAYING =================
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

    private void OnPlaying()
    {
        playerActionUI.SetActive(true);
        roleUI.Show();

        playingTimerCoroutine = StartCoroutine(PlayingTimerCoroutine());
        pingCoroutine = StartCoroutine(PingUI());

        Debug.Log("Game Start!");
    }

    // ================= PING SYSTEM =================
    private IEnumerator PingUI()
    {
        while (true)
        {
            // cooldown 1 -> 0
            float t = 0f;

            while (t < pingInterval)
            {
                t += Time.deltaTime;
                OnPingCooldownUpdate?.Invoke(1f - (t / pingInterval));
                yield return null;
            }

            OnPingCooldownUpdate?.Invoke(0f);

            // get hiders
            List<RoleComponent> hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);

            // CHỈ HIỂN THỊ NẾU BẢN THÂN KHÔNG PHẢI SEEKER (Mọi Hider đều tự nhìn thấy nhau)
            bool playerIsSeeker = player.GetComponent<RoleComponent>().Role == GameRole.Seeker;
            if (!playerIsSeeker)
            {
                // Quét qua danh sách toàn bộ các Hider hiện tại (bao gồm cả Player lẫn Bot)
                foreach (var hider in hiders)
                {
                    // Tự tìm script HiderPingIcon nằm trên nhân vật Hider đó và bật lên
                    HiderPingIcon pingIcon = hider.GetComponentInChildren<HiderPingIcon>();
                    if (pingIcon != null)
                    {
                        pingIcon.Show();
                    }
                }
            }

            OnPingStart?.Invoke();
            OnPingHiders?.Invoke(hiders);

            yield return new WaitForSeconds(pingDuration);

            // TẮT TẤT CẢ ICON KHI HẾT DURATION
            foreach (var hider in hiders)
            {
                if (hider != null)
                {
                    HiderPingIcon pingIcon = hider.GetComponentInChildren<HiderPingIcon>();
                    if (pingIcon != null)
                    {
                        pingIcon.Hide();
                    }
                }
            }

            OnPingEnd?.Invoke();
            OnPingCooldownUpdate?.Invoke(1f);
        }
    }

    // ================= LOCK =================
    private void LockSeekers(bool locked)
    {
        if (player.GetComponent<RoleComponent>().Role == GameRole.Seeker)
            player.movement.enabled = !locked;

        foreach (var bot in bots)
        {
            if (bot.GetComponent<RoleComponent>().Role == GameRole.Seeker)
                bot.Agent.isStopped = locked;
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

        // Tắt hết icon của tất cả các Hider khi game kết thúc
        List<RoleComponent> hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);
        foreach (var hider in hiders)
        {
            if (hider != null)
            {
                hider.GetComponentInChildren<HiderPingIcon>()?.Hide();
            }
        }

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