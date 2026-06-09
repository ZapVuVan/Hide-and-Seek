using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UseItemButton : MonoBehaviour, IPointerDownHandler
{
    [Header("UI")]
    [SerializeField] private Button button;
    public Image iconImage;
    public TextMeshProUGUI chargeText;

    private bool isCooldown;
    private PowerData powerData;
    private PlayerMovement player;

    public void Setup(PowerData data) // PowerData là abstract base → nhận được HealPowerData, SpeedBoostPowerData, FreezePowerData
    {
        powerData = data;
        player = FindObjectOfType<PlayerMovement>();
        if (iconImage != null) iconImage.sprite = data.icon;
        RefreshChargeText();
    }

    public void RefreshChargeText()
    {
        if (chargeText == null || powerData == null) return;
        int charges = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetCharges(powerData.itemId) : 0;
        chargeText.text = $"x{charges}";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCooldown || powerData == null || player == null) return;

        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            NotificationUseEffectUI.Instance.Show();
            StartCoroutine(Notification());
            return;
        }

        int charges = InventoryManager.Instance.GetCharges(powerData.itemId);
        if (charges <= 0)
        {
            Debug.Log("[UseItemButton] Hết charge!");
            return;
        }

        // Dùng charge và apply effect
        InventoryManager.Instance.UseCharge(powerData.itemId);
        powerData.Apply(player.gameObject);
        RefreshChargeText();

        // Ẩn nút nếu hết charge
        if (InventoryManager.Instance.GetCharges(powerData.itemId) <= 0)
        {
            gameObject.SetActive(false);
            // Hết charge và ẩn nút rồi thì return luôn, không chạy CooldownRoutine bên dưới nữa
            return;
        }
        else
        {
            // Còn charge thì mới cần chạy Cooldown để chờ bấm phát tiếp theo
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        button.interactable = false;
        float timer = powerData.cooldown;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        button.interactable = true;
        isCooldown = false;
    }

    private IEnumerator Notification()
    {
        float timer = NotificationUseEffectUI.Instance.GetTimeHide();
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        NotificationUseEffectUI.Instance.Hide();
    }
}