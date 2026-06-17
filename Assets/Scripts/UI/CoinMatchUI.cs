using UnityEngine;
using TMPro;
using DG.Tweening;

public class MatchCoinUI : MonoBehaviour
{
    public static MatchCoinUI Instance { get; private set; }

    [SerializeField] private TMP_Text coinText;
    [SerializeField] private RectTransform coinIcon;
    [SerializeField] private TMP_Text floatingText; // ← TMP_Text cố định trong Scene

    private int matchCoin = 0;
    private RectTransform floatingRT;

    private void Awake()
    {
        Instance = this;
        floatingRT = floatingText.GetComponent<RectTransform>();

        // Ẩn ban đầu
        floatingText.alpha = 0f;
    }

    private void Start() => UpdateDisplay();

    public void AddCoin(int amount)
    {
        int from = matchCoin;
        matchCoin += amount;

        // Đếm tăng dần
        DOTween.To(() => from, x =>
        {
            coinText.text = x.ToString();
        }, matchCoin, 0.3f).SetEase(Ease.OutQuad);

        // Icon nảy
        coinIcon?.DOKill();
        coinIcon?.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);

        // Floating text
        ShowFloatingText($"+{amount}");
    }

    public void ResetCoin()
    {
        matchCoin = 0;
        coinText.text = "0";
        floatingText.alpha = 0f;
    }

    private void UpdateDisplay() => coinText.text = matchCoin.ToString();

    private void ShowFloatingText(string content)
    {
        // Kill animation cũ nếu đang chạy
        floatingText.DOKill();
        floatingRT.DOKill();

        floatingText.text = content;
        floatingText.color = new Color(1f, 0.9f, 0f, 1f);

        // Reset vị trí
        floatingRT.anchoredPosition = Vector2.zero;

        // Bay lên + fade out
        floatingRT.DOAnchorPosY(80f, 0.8f).SetEase(Ease.OutCubic);
        floatingText.DOFade(0f, 0.8f).SetEase(Ease.InQuad);
    }
}