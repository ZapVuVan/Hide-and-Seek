// ==================== GameWinUI.cs ====================
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameWinUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text seekerNameText;
    [SerializeField] private TMP_Text hiderNameText;
    [SerializeField] private TMP_Text coinText;

    [Header("Color")]
    [SerializeField] private Color colorVictory = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color colorDefeat = new Color(1f, 0.42f, 0.42f);

    private int matchCoin;
    private Coroutine returnCoroutine;

    private void Start() => panel.SetActive(false);

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnHidingPhaseStart += HandleHidingPhaseStart;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnHidingPhaseStart -= HandleHidingPhaseStart;
    }

    private void HandleHidingPhaseStart(float duration, bool isSeeker)
    {
        matchCoin = 0;
    }

    public void AddMatchCoin(int amount) => matchCoin += amount;

    public void Show(GameRole winner) // ← SỬA: nhận winner
    {
        bool seekersWon = winner == GameRole.Seeker;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        var playerRole = playerObj != null ? playerObj.GetComponent<RoleComponent>() : null;
        bool playerWon = playerRole != null && playerRole.Role == winner;

        titleText.text = seekersWon ? "Seekers won!" : "Hiders won!";
        titleText.color = playerWon ? colorVictory : colorDefeat;

        var bestSeeker = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null &&
                        t.GetComponent<RoleComponent>()?.Role == GameRole.Seeker)
            .OrderByDescending(t => t.KillCount)
            .FirstOrDefault();
        seekerNameText.text = bestSeeker != null ? bestSeeker.gameObject.name : "—";

        var bestHider = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null && t.WasHider)
            .OrderByDescending(t => t.SurvivalTime)
            .ThenByDescending(t => t.DistanceTraveled)
            .FirstOrDefault();
        hiderNameText.text = bestHider != null ? bestHider.gameObject.name : "—";

        coinText.text = matchCoin.ToString();

        panel.SetActive(true);

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnToSelectMap(5f));
    }

    private IEnumerator ReturnToSelectMap(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("SelectMap");
    }

    public void Hide()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        panel.SetActive(false);
    }
}