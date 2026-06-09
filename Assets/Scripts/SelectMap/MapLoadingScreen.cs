using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapLoadingScreen : MonoBehaviour
{
    public static MapLoadingScreen Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("UI")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button exitButton;

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
    private Coroutine popTextCoroutine;

    private Vector3 textOriginalScale;
    private Vector3 countOriginalScale;

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

        // Lưu lại scale gốc chuẩn ban đầu để tránh lỗi méo font khi chạy hiệu ứng Pop liên tục
        if (statusText != null) textOriginalScale = statusText.transform.localScale;
        if (playerCountText != null) countOriginalScale = playerCountText.transform.localScale;
    }

    public void StartLoading(string sceneName)
    {
        targetScene = sceneName;

        // Dọn sạch toàn bộ các Coroutine cũ đang chạy dở trước đó
        StopAllLoadingCoroutines();

        panel.SetActive(true);

        playerCountText.text = $"1 / {maxPlayers}";
        statusText.text = "Waiting for players";

        if (progressFill != null)
            progressFill.fillAmount = 1f / maxPlayers;

        if (exitButton != null)
        {
            exitButton.interactable = true;
            exitButton.gameObject.SetActive(true);
        }

        StartCoroutine(ShowPanelAnimation());

        dotsCoroutine = StartCoroutine(AnimateDots());
        loadingCoroutine = StartCoroutine(FakeLoadingCoroutine());
    }

    public void OnClickExit()
    {
        // 1. Dừng ngay lập tức tất cả các tiến trình chạy ngầm
        StopAllCoroutines();

        loadingCoroutine = null;
        dotsCoroutine = null;
        popTextCoroutine = null;

        // 2. Trả lại scale chuẩn cho các UI Text và Panel
        if (statusText != null) statusText.transform.localScale = textOriginalScale;
        if (playerCountText != null) playerCountText.transform.localScale = countOriginalScale;
        if (panel != null) panel.transform.localScale = Vector3.one;

        // 3. Ẩn màn hình Loading ngay lập tức
        if (panel != null) panel.SetActive(false);

        // 4. Giải quyết lỗi kẹt Trigger: Đẩy nhẹ Player lùi lại ra khỏi bục chọn Map
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Tạm thời tắt CharacterController để việc can thiệp transform.position không bị xung đột vật lý
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Dịch chuyển nhân vật lùi về phía sau 1.5 mét dựa theo hướng mặt hiện tại
            player.transform.position -= player.transform.forward * 1.5f;

            // Bật lại CharacterController sau khi dịch chuyển thành công
            if (cc != null) cc.enabled = true;
        }

        Debug.Log("Đã hủy Loading, tắt Panel và đẩy Player ra khỏi bục Trigger thành công!");
    }

    private void StopAllLoadingCoroutines()
    {
        if (loadingCoroutine != null) StopCoroutine(loadingCoroutine);
        if (dotsCoroutine != null) StopCoroutine(dotsCoroutine);
        if (popTextCoroutine != null) StopCoroutine(popTextCoroutine);

        loadingCoroutine = null;
        dotsCoroutine = null;
        popTextCoroutine = null;
    }

    private IEnumerator FakeLoadingCoroutine()
    {
        int currentPlayers = 1;
        float elapsed = 0f;
        float totalWait = Random.Range(minWaitTime, maxWaitTime);

        // GIAI ĐOẠN 1: Chờ lấp đầy phòng giả lập
        while (elapsed < totalWait && currentPlayers < maxPlayers)
        {
            yield return new WaitForSeconds(joinInterval);
            elapsed += joinInterval;

            int addPlayers = Random.Range(1, 3);
            currentPlayers = Mathf.Min(currentPlayers + addPlayers, maxPlayers);

            playerCountText.text = $"{currentPlayers} / {maxPlayers}";

            if (popTextCoroutine != null) StopCoroutine(popTextCoroutine);
            popTextCoroutine = StartCoroutine(PopAnimation(playerCountText.transform, countOriginalScale));

            if (progressFill != null)
            {
                progressFill.fillAmount = (float)currentPlayers / maxPlayers;
            }
        }

        // Dừng hiệu ứng chạy 3 dấu chấm để trả tự do hoàn toàn cho statusText trước khi đếm ngược
        if (dotsCoroutine != null)
        {
            StopCoroutine(dotsCoroutine);
            dotsCoroutine = null;
        }

        statusText.text = "Room Full!";
        if (popTextCoroutine != null) StopCoroutine(popTextCoroutine);
        popTextCoroutine = StartCoroutine(PopAnimation(statusText.transform, textOriginalScale));

        yield return new WaitForSeconds(0.6f);

        // Khóa nút Exit khi bắt đầu đếm ngược vào trận (Ngăn việc hủy trận giây cuối)
        if (exitButton != null) exitButton.interactable = false;

        // GIAI ĐOẠN 2: Countdown khởi động game
        for (int i = 3; i >= 1; i--)
        {
            statusText.text = $"Starting in {i}";

            if (popTextCoroutine != null) StopCoroutine(popTextCoroutine);
            popTextCoroutine = StartCoroutine(PopAnimation(statusText.transform, textOriginalScale));

            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator AnimateDots()
    {
        while (true)
        {
            statusText.text = "Waiting for players.";
            yield return new WaitForSeconds(0.4f);

            statusText.text = "Waiting for players..";
            yield return new WaitForSeconds(0.4f);

            statusText.text = "Waiting for players...";
            yield return new WaitForSeconds(0.4f);
        }
    }

    private IEnumerator PopAnimation(Transform target, Vector3 originalScale)
    {
        Vector3 targetScale = originalScale * 1.15f;
        float timer = 0f;

        while (timer < 0.08f)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, targetScale, timer / 0.08f);
            yield return null;
        }

        timer = 0f;
        while (timer < 0.08f)
        {
            timer += Time.deltaTime;
            target.localScale = Vector3.Lerp(targetScale, originalScale, timer / 0.08f);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private IEnumerator ShowPanelAnimation()
    {
        Transform panelTransform = panel.transform;
        Vector3 startScale = Vector3.one * 0.8f;
        Vector3 endScale = Vector3.one;

        panelTransform.localScale = startScale;
        float timer = 0f;

        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            panelTransform.localScale = Vector3.Lerp(startScale, endScale, timer / 0.15f);
            yield return null;
        }

        panelTransform.localScale = endScale;
    }
}