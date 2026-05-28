// GameManager.cs
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

    [Header("Config")]
    [SerializeField] private float roleAssignDelay = 3f;
    [SerializeField] private float hidingPhaseDuration = 15f;

    private GameState currentState;

    private void Awake() => Instance = this;

    private void Start()
    {
        TransitionToState(GameState.AssigningRoles);
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

        int seekerIndex = Random.Range(0, allCharacters.Count);
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

        // Khóa Seeker
        LockSeekers(true);

        // Nếu player là Seeker thì hiện UI che mắt
        if (playerIsSeeker)
            hidingPhaseUI.Show();

        // Đếm ngược
        float timeLeft = hidingPhaseDuration;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            hidingPhaseUI.UpdateTimer(timeLeft);
            yield return null;
        }

        // Hết thời gian
        LockSeekers(false);
        hidingPhaseUI.Hide();

        TransitionToState(GameState.Playing);
    }

    private void LockSeekers(bool locked)
    {
        // Lock player nếu là Seeker
        if (player.GetComponent<RoleComponent>().Role == GameRole.Seeker)
            player.movement.enabled = !locked;

        // Lock bot Seeker
        foreach (var bot in bots)
        {
            if (bot.GetComponent<RoleComponent>().Role == GameRole.Seeker)
                bot.Agent.isStopped = locked;
        }
    }

    private void OnPlaying()
    {
        roleUI.Refresh();
        Debug.Log("Game bắt đầu!");
    }

    private void OnGameEnd()
    {
        Debug.Log("Game kết thúc!");
    }

    public void CheckGameEnd()
    {
        int hiderCount = RoleManager.Instance.CountByRole(GameRole.Hider);
        if (hiderCount <= 0)
        {
            Debug.Log("Seeker thắng!");
            TransitionToState(GameState.GameEnd);
        }
    }
}