// GameManager.cs
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

    [Header("Config")]
    [SerializeField] private float roleAssignDelay = 3f;
    [SerializeField] private float hidingPhaseDuration = 15f;
    [SerializeField] private float playingDuration = 90f;

    private GameState currentState;
    public GameState CurrentState => currentState;

    private Coroutine playingTimerCoroutine;

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

    private IEnumerator AssigningRolesCoroutine()
    {
        yield return new WaitForSeconds(roleAssignDelay);

        List<GameObject> allCharacters = new List<GameObject>();
        allCharacters.Add(player.gameObject);
        foreach (var bot in bots)
            allCharacters.Add(bot.gameObject);

        int seekerIndex = UnityEngine.Random.Range(0, allCharacters.Count);
        for (int i = 0; i < allCharacters.Count; i++)
        {
            GameRole role = i == seekerIndex ? GameRole.Seeker : GameRole.Hider;
            allCharacters[i].GetComponent<RoleComponent>()?.SetRole(role);
        }

        yield return null;
        TransitionToState(GameState.HidingPhase);
    }

    private IEnumerator HidingPhaseCoroutine()
    {
        bool playerIsSeeker = player.GetComponent<RoleComponent>().Role == GameRole.Seeker;
        LockSeekers(true);

        if (playerIsSeeker)
            hidingPhaseUI.Show();

        float timeLeft = hidingPhaseDuration;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            hidingPhaseUI.UpdateTimer(timeLeft);
            yield return null;
        }

        LockSeekers(false);
        hidingPhaseUI.Hide();
        TransitionToState(GameState.Playing);
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

    private void OnPlaying()
    {
        roleUI.Show();
        playingTimerCoroutine = StartCoroutine(PlayingTimerCoroutine());
        Debug.Log("Game bắt đầu!");
    }

    private void OnGameEnd()
    {
        if (playingTimerCoroutine != null)
        {
            StopCoroutine(playingTimerCoroutine);
            playingTimerCoroutine = null;
        }
        timePlayGameUI.Hide();
        gameWinUI.Show();
    }

    public void CheckGameEnd()
    {
        if (currentState != GameState.Playing) return;

        int hiderCount = RoleManager.Instance.CountByRole(GameRole.Hider);
        Debug.Log($"CheckGameEnd | hiderCount: {hiderCount} | state: {currentState}");

        if (hiderCount <= 0)
            TransitionToState(GameState.GameEnd);
    }
}