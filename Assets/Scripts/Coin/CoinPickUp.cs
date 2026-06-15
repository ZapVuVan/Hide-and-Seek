using UnityEngine;
using DG.Tweening;

public class CoinPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int coinValue = 2;
    [SerializeField] private float floatHeight = 0.3f;
    [SerializeField] private float floatSpeed = 1.2f;

    private bool collected = false;
    private Vector3 startPos;
    private CoinSpawner spawner;

    private void Start()
    {
        startPos = transform.position;
        spawner = FindObjectOfType<CoinSpawner>();

        // Hiệu ứng lên xuống nhẹ nhàng liên tục
        transform.DOMoveY(startPos.y + floatHeight, floatSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // -1 = lặp mãi, Yoyo = lên rồi xuống

        // Xoay coin liên tục (optional, trông đẹp hơn)
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // Lấy CoinManager để cộng coin
        if (other.TryGetComponent<CoinManager>(out var coinManager))
        {
            coinManager.AddCoin(coinValue);
        }

        // Báo spawner xóa khỏi danh sách
        spawner?.RemoveCoin(gameObject);

        // Kill tween đang chạy trước khi play animation collect
        transform.DOKill();

        // Animation collect: bay lên + fade out + scale nhỏ
        Sequence collectSeq = DOTween.Sequence();
        collectSeq.Append(transform.DOMoveY(transform.position.y + 1f, 0.4f).SetEase(Ease.OutCubic));
        collectSeq.Join(transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));

        // Fade out nếu có Renderer
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Material mat = rend.material;
            if (mat.HasProperty("_Color"))
            {
                collectSeq.Join(
                    DOTween.To(() => mat.color, x => mat.color = x,
                        new Color(mat.color.r, mat.color.g, mat.color.b, 0f), 0.4f)
                );
            }
        }

        collectSeq.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill(); // dọn tween khi bị destroy
    }
}