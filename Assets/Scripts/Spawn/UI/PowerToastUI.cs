using UnityEngine;
using TMPro;
using DG.Tweening;

public class PowerToastUI : MonoBehaviour
{
    public static PowerToastUI Instance { get; private set; }

    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // Hide on start
        messageText.alpha = 0f;
    }

    public void Show(string itemName)
    {
        messageText.text = $"You received {itemName}!";
        PlayAnimation();
    }

    public void ShowFail(string message)
    {
        messageText.text = message;
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        DOTween.Kill(messageText);
        DOTween.Kill(transform);

        messageText.alpha = 0f;
        transform.localPosition = new Vector3(0, -30f, 0);

        // Fade in and slide up
        messageText.DOFade(1f, 0.2f);
        transform.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutBack)
                 .OnComplete(() =>
                 {
                     // Wait 1.5s then fade out and slide up
                     DOVirtual.DelayedCall(1.5f, () =>
                     {
                         messageText.DOFade(0f, 0.3f);
                         transform.DOLocalMoveY(30f, 0.3f).SetEase(Ease.InBack);
                     });
                 });
    }
}