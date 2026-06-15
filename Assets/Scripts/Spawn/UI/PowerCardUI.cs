using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PowerCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text priceText;

    private PowerData powerData;

    public void Setup(PowerData data)
    {
        powerData = data;
        nameText.text = data.itemName;
        iconImage.sprite = data.icon;
        priceText.text = data.price.ToString();
        background.color = data.GetRarityColor();
    }

    public void OnClickBuy()
    {
        if (powerData == null) return;

        var result = InventoryManager.Instance.TryBuy(powerData.itemId);

        switch (result)
        {
            case InventoryManager.BuyResult.Success:
                PlayBuyEffect();
                PowerToastUI.Instance.Show(powerData.itemName);
                break;
            case InventoryManager.BuyResult.NotEnoughCoins:
                PlayFailEffect();
                PowerToastUI.Instance.ShowFail("Not enough coins!");
                break;
        }
    }

    private void PlayBuyEffect()
    {
        DOTween.Kill(transform);
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, vibrato: 1, elasticity: 0.5f);

        // Flash màu trắng rồi về màu gốc
        Color original = background.color;
        background.DOColor(Color.white, 0.1f)
                  .OnComplete(() => background.DOColor(original, 0.2f));
    }

    private void PlayFailEffect()
    {
        DOTween.Kill(transform);
        transform.localScale = Vector3.one;
        // Lắc ngang
        transform.DOShakePosition(0.3f, strength: new Vector3(5f, 0, 0), vibrato: 20, randomness: 0);
    }
}