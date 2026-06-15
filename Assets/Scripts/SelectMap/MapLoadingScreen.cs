using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class MapLoadingScreen : MonoBehaviour
{
    public static MapLoadingScreen Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("UI")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button exitButton;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;

    [Header("Progress")]
    [SerializeField] private Image progressFill;

    [Header("Settings")]
    [SerializeField] private int maxPlayers = 8;
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 7f;
    [SerializeField] private float joinInterval = 0.6f;

    private string targetScene;
    private Coroutine loadingCoroutine;
    private Coroutine dotsCoroutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panel.AddComponent<CanvasGroup>();

        // Ẩn bằng alpha thay vì SetActive(false) để Coroutine không bị lỗi inactive
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void StartLoading(string sceneName, Sprite mapBackground = null)
    {
        targetScene = sceneName;
        StopAllLoadingCoroutines();

        if (backgroundImage != null)
        {
            if (mapBackground != null)
            {
                backgroundImage.sprite = mapBackground;
                backgroundImage.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }
        }

        playerCountText.text = $"1 / {maxPlayers}";
        statusText.text = "Waiting for players";

        if (progressFill != null)
            progressFill.fillAmount = 1f / maxPlayers;

        if (exitButton != null)
            exitButton.gameObject.SetActive(true);

        StartCoroutine(ShowPanelAnimation());
        dotsCoroutine = StartCoroutine(AnimateDots());
        loadingCoroutine = StartCoroutine(FakeLoadingCoroutine());
    }

    public void OnClickExit()
    {
        StopAllCoroutines();

        DOTween.Kill(statusText.transform);
        DOTween.Kill(playerCountText.transform);
        if (progressFill != null) DOTween.Kill(progressFill);

        statusText.transform.localScale = Vector3.one;
        playerCountText.transform.localScale = Vector3.one;

        loadingCoroutine = null;
        dotsCoroutine = null;

        canvasGroup.DOFade(0f, 0.15f).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position -= player.transform.forward * 1.5f;
            if (cc != null) cc.enabled = true;
        }
    }

    private void StopAllLoadingCoroutines()
    {
        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        if (dotsCoroutine != null) StopCoroutine(dotsCoroutine);
        loadingCoroutine = null;
        dotsCoroutine = null;

        DOTween.Kill(statusText.transform);
        DOTween.Kill(playerCountText.transform);
        if (progressFill != null) DOTween.Kill(progressFill);
    }

    private IEnumerator FakeLoadingCoroutine()
    {
        int currentPlayers = 1;
        float elapsed = 0f;
        float totalWait = Random.Range(minWaitTime, maxWaitTime);

        while (elapsed < totalWait && currentPlayers < maxPlayers)
        {
            yield return new WaitForSeconds(joinInterval);
            elapsed += joinInterval;

            int addPlayers = Random.Range(1, 3);
            currentPlayers = Mathf.Min(currentPlayers + addPlayers, maxPlayers);

            playerCountText.text = $"{currentPlayers} / {maxPlayers}";
            PopAnimation(playerCountText.transform);

            if (progressFill != null)
            {
                DOTween.Kill(progressFill);
                progressFill.DOFillAmount((float)currentPlayers / maxPlayers, 0.3f)
                            .SetEase(Ease.OutCubic);
            }
        }

        if (dotsCoroutine != null)
        {
            StopCoroutine(dotsCoroutine);
            dotsCoroutine = null;
        }

        statusText.text = "Room Full!";
        PopAnimation(statusText.transform);

        yield return new WaitForSeconds(0.6f);

        // Tắt hẳn nút Exit khi bắt đầu countdown
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        for (int i = 3; i >= 1; i--)
        {
            statusText.text = $"Starting in {i}";
            PopAnimation(statusText.transform);
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator AnimateDots()
    {
        string[] dots = { ".", "..", "..." };
        int index = 0;
        while (true)
        {
            statusText.text = $"Waiting for players{dots[index % 3]}";
            index++;
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void PopAnimation(Transform target)
    {
        DOTween.Kill(target);
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * 0.15f, 0.25f, vibrato: 1, elasticity: 0.5f);
    }

    private IEnumerator ShowPanelAnimation()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float timer = 0f;
        float duration = 0.2f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}