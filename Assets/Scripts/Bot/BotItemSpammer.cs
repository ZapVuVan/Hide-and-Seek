using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotItemSpammer : MonoBehaviour
{
    [Header("Pool of Effects")]
    [SerializeField] private List<PowerData> availableEffects;

    [Header("Timing & Logic")]
    [SerializeField] private float interval = 15f;

    private void Start()
    {
        StartCoroutine(SpamItemLoop());
    }

    private IEnumerator SpamItemLoop()
    {
        yield return new WaitForSeconds(5f);

        while (true)
        {
            yield return new WaitForSeconds(interval);

            BotController[] allBots = FindObjectsOfType<BotController>();
            if (allBots.Length == 0 || availableEffects.Count == 0) continue;

            // ✅ Chỉ lấy Bot có role Hider
            List<BotController> hiderBots = new List<BotController>();
            foreach (var bot in allBots)
            {
                var role = bot.GetComponent<RoleComponent>();
                if (role != null && role.Role == GameRole.Hider)
                    hiderBots.Add(bot);
            }
            if (hiderBots.Count == 0) continue;

            int botsToTargetCount = Random.Range(1, 4);
            botsToTargetCount = Mathf.Min(botsToTargetCount, hiderBots.Count);

            List<BotController> shuffledBots = new List<BotController>(hiderBots);
            ShuffleList(shuffledBots);

            for (int i = 0; i < botsToTargetCount; i++)
            {
                BotController luckyBot = shuffledBots[i];
                PowerData randomEffect = availableEffects[Random.Range(0, availableEffects.Count)];

                if (randomEffect != null && luckyBot != null)
                {
                    Debug.Log($"[Hệ thống] Đã ném ngẫu nhiên item {randomEffect.itemName} cho Bot: {luckyBot.gameObject.name}");
                    randomEffect.Apply(luckyBot.gameObject);
                }
            }
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}