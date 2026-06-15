using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        CoinManager.Instance.OnCoinChanged += UpdateUI;
        UpdateUI(CoinManager.Instance.GetCoin());
    }

    private void UpdateUI(int coin)
    {
        coinText.text = "" + coin;
    }

    private void OnDestroy()
    {
        CoinManager.Instance.OnCoinChanged -= UpdateUI;
    }
}