using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapSelectManager : MonoBehaviour
{
    [SerializeField] private MapZone zone1;
    [SerializeField] private MapZone zone2;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float waitTime = 15f;

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

        int map1Votes = zone1.GetPlayerCount();
        int map2Votes = zone2.GetPlayerCount();

        string selectedScene;

        if (map1Votes > map2Votes)
        {
            selectedScene = zone1.sceneName;
        }
        else if (map2Votes > map1Votes)
        {
            selectedScene = zone2.sceneName;
        }
        else
        {
            // Hoà nhau -> random 1 trong 2 map
            selectedScene = Random.value < 0.5f
                ? zone1.sceneName
                : zone2.sceneName;
        }

        Debug.Log($"Map selected: {selectedScene}");
        SceneManager.LoadScene(selectedScene);
    }
}