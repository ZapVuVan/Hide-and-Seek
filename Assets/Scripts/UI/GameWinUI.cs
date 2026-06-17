// ==================== GameWinUI.cs ====================
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

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

    [Header("Coin Animation")]
    [SerializeField] private RectTransform coinIcon;

    private int matchCoin;
    private Coroutine returnCoroutine;

    // ================= LIFECYCLE =================
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

    // ================= MATCH COIN =================
    private void HandleHidingPhaseStart(float duration, bool isSeeker)
    {
        matchCoin = 0;
        MatchCoinUI.Instance?.ResetCoin();
    }

    public void AddMatchCoin(int amount) => matchCoin += amount;

    // ================= SHOW =================
    public void Show(GameRole winner)
    {
        bool seekersWon = winner == GameRole.Seeker;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        var playerRole = playerObj != null ? playerObj.GetComponent<RoleComponent>() : null;
        bool playerWon = playerRole != null && playerRole.Role == winner;

        // Title
        titleText.text = seekersWon ? "Seekers won!" : "Hiders won!";
        titleText.color = playerWon ? colorVictory : colorDefeat;

        // Best Seeker
        var bestSeeker = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null &&
                        t.GetComponent<RoleComponent>()?.Role == GameRole.Seeker)
            .OrderByDescending(t => t.KillCount)
            .FirstOrDefault();
        seekerNameText.text = bestSeeker != null ? bestSeeker.gameObject.name : "—";

        // Best Hider
        var bestHider = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null && t.WasHider)
            .OrderByDescending(t => t.SurvivalTime)
            .ThenByDescending(t => t.DistanceTraveled)
            .FirstOrDefault();
        hiderNameText.text = bestHider != null ? bestHider.gameObject.name : "—";

        // Coin
        if (matchCoin > 0)
            CoinManager.Instance.AddCoin(matchCoin);

        coinText.text = "0";
        panel.SetActive(true);

        // Animate coin đếm lên sau 0.5s để panel kịp hiện
        DOVirtual.DelayedCall(0.5f, () => AnimateCoinCount(matchCoin));

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnToSelectMap(5f));
    }

    // ================= COIN ANIMATION =================
    private void AnimateCoinCount(int total)
    {
        if (total <= 0)
        {
            coinText.text = "0";
            return;
        }

        int current = 0;

        DOTween.To(() => current, x =>
        {
            current = x;
            coinText.text = x.ToString();
        }, total, 1.5f)
        .SetEase(Ease.OutQuart)
        .OnComplete(() =>
        {
            // Nảy icon
            coinIcon?.DOKill();
            coinIcon?.DOPunchScale(Vector3.one * 0.4f, 0.4f, 6, 0.5f);

            // Flash màu vàng rồi trở về trắng
            coinText.DOColor(new Color(1f, 0.84f, 0f), 0.2f)
                    .OnComplete(() => coinText.DOColor(Color.white, 0.3f));
        });
    }

    // ================= HIDE =================
    public void Hide()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        DOTween.Kill(coinText);
        DOTween.Kill(coinIcon);

        panel.SetActive(false);
    }

    // ================= RETURN =================
    private IEnumerator ReturnToSelectMap(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("SelectMap");
    }
}