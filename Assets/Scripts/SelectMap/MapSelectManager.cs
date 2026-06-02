// MapSelectManager.cs - gắn vào MapSelect object
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapSelectManager : MonoBehaviour
{
    [SerializeField] private MapZone zone1; // BoxCheck_1
    [SerializeField] private MapZone zone2; // BoxCheck_2
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float waitTime = 15f;
    [SerializeField] private string defaultScene = "GamePlay"; // nếu k đứng trong box nào

    private void Start()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        float timeLeft = waitTime;
        while (timeLeft > 0)
        {
            countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // ✅ Hết giờ → check box
        if (zone1.HasPlayer())
            SceneManager.LoadScene(zone1.sceneName);
        else if (zone2.HasPlayer())
            SceneManager.LoadScene(zone2.sceneName);
        else
            SceneManager.LoadScene(defaultScene); // random hoặc default
    }
}