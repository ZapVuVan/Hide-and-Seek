using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MapSelectUI : MonoBehaviour
{
    public static MapSelectUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Join Button")]
    [SerializeField] private GameObject joinButtonObj;
    [SerializeField] private Button joinButton;

    [Header("Buy Button")]
    [SerializeField] private GameObject buyButtonObj;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyCostText;

    [Header("Not Enough Coin Popup")]
    [SerializeField] private GameObject notEnoughCoinPopup;
    [SerializeField] private float popupDuration = 2f;

    private MapTriggerZone currentZone;
    private Coroutine popupCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null)
            panel.SetActive(false);

        if (notEnoughCoinPopup != null)
            notEnoughCoinPopup.SetActive(false);
    }

    public void ShowJoinButton(MapTriggerZone zone)
    {
        currentZone = zone;
        if (panel != null)
            panel.SetActive(true);
        RefreshButtons();
    }

    public void HideJoinButton()
    {
        if (panel != null)
            panel.SetActive(false);

        // Tắt popup và cancel coroutine khi rời zone
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }

        if (notEnoughCoinPopup != null)
            notEnoughCoinPopup.SetActive(false);

        currentZone = null;
    }

    private void RefreshButtons()
    {
        if (currentZone == null)
            return;

        bool locked = currentZone.IsLocked;
        joinButtonObj.SetActive(!locked);
        buyButtonObj.SetActive(locked);

        if (locked && buyCostText != null)
            buyCostText.text = $"{currentZone.UnlockCost}";
    }

    public void OnClickJoin()
    {
        Debug.Log("JOIN BUTTON CLICKED");
        if (currentZone == null) return;
        if (currentZone.IsLocked) return;

        Debug.Log($"[MAP] Join {currentZone.SceneName}");

        if (MapLoadingScreen.Instance != null)
            MapLoadingScreen.Instance.StartLoading(currentZone.SceneName);

        panel.SetActive(false);
    }

    public void OnClickBuy()
    {
        if (currentZone == null) return;
        if (!currentZone.IsLocked) return;

        int playerCoin = CoinManager.Instance.coin;

        if (playerCoin < currentZone.UnlockCost)
        {
            ShowNotEnoughCoinPopup();
            return;
        }

        CoinManager.Instance.SpendCoin(currentZone.UnlockCost);
        currentZone.IsLocked = false;

        PlayerPrefs.SetInt($"Map_Unlocked_{currentZone.SceneName}", 1);
        PlayerPrefs.Save();

        Debug.Log($"[MAP] Unlock {currentZone.SceneName}");
        RefreshButtons();
    }

    private void ShowNotEnoughCoinPopup()
    {
        if (notEnoughCoinPopup == null) return;

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(PopupCoroutine());
    }

    private IEnumerator PopupCoroutine()
    {
        notEnoughCoinPopup.SetActive(true);
        yield return new WaitForSeconds(popupDuration);
        notEnoughCoinPopup.SetActive(false);
        popupCoroutine = null;
    }

}