using System.Linq;
using UnityEngine;
using TMPro;

public class GameWinUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;      // "Seekers won!" / "Hiders won!"
    [SerializeField] private TMP_Text seekerNameText; // tên Best Seeker
    [SerializeField] private TMP_Text hiderNameText;  // tên Best Hider
    [SerializeField] private TMP_Text coinText;       // coin kiếm trong trận

    [Header("Color")]
    [SerializeField] private Color colorVictory = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color colorDefeat = new Color(1f, 0.42f, 0.42f);

    // Coin kiếm trong trận — cộng dồn qua AddMatchCoin()
    private int matchCoin;

    private void Start()
    {
        panel.SetActive(false);
    }

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

    // Reset coin đếm mỗi trận mới
    private void HandleHidingPhaseStart(float duration, bool isSeeker)
    {
        matchCoin = 0;
    }

    // Gọi hàm này mỗi khi player nhận coin trong trận
    // (thay thế hoặc gọi thêm bên cạnh CoinManager.AddCoin)
    public void AddMatchCoin(int amount)
    {
        matchCoin += amount;
    }

    public void Show()
    {
        bool seekersWon = RoleManager.Instance != null &&
                          RoleManager.Instance.CountByRole(GameRole.Hider) <= 0;
        GameRole winningSide = seekersWon ? GameRole.Seeker : GameRole.Hider;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        var playerRole = playerObj != null ? playerObj.GetComponent<RoleComponent>() : null;
        bool playerWon = playerRole != null && playerRole.Role == winningSide;
        titleText.text = seekersWon ? "Seekers won!" : "Hiders won!";
        titleText.color = playerWon ? colorVictory : colorDefeat;

        // 4. Best Seeker — kill nhiều nhất
        var bestSeeker = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null &&
                        t.GetComponent<RoleComponent>()?.Role == GameRole.Seeker)
            .OrderByDescending(t => t.KillCount)
            .FirstOrDefault();

        seekerNameText.text = bestSeeker != null ? bestSeeker.gameObject.name : "—";

        // 5. Best Hider — dùng WasHider để không bỏ sót người bị bắt
        //    sống lâu nhất → tiebreak chạy xa nhất
        var bestHider = Object.FindObjectsOfType<PlayerStatsTracker>()
            .Where(t => t != null && t.WasHider)
            .OrderByDescending(t => t.SurvivalTime)
            .ThenByDescending(t => t.DistanceTraveled)
            .FirstOrDefault();

        hiderNameText.text = bestHider != null ? bestHider.gameObject.name : "—";

        // 6. Coin kiếm trong trận
        coinText.text = matchCoin.ToString();

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}