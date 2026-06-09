using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class BotSpawner : MonoBehaviour
{
    [Header("Bot")]
    public GameObject botPrefab;

    [Header("Wandering Bots - Luôn tồn tại trên map")]
    public int minWanderingBots = 3;
    public int maxWanderingBots = 6;

    [Header("MapSelector Bots - Thi thoảng xuất hiện")]
    public int minMapSelectorBots = 1;  // Tối thiểu bao nhiêu con đang chọn map
    public int maxMapSelectorBots = 3;  // Tối đa bao nhiêu con cùng lúc
    public float minSpawnInterval = 3f; // Tối thiểu bao lâu spawn thêm 1 con
    public float maxSpawnInterval = 8f; // Tối đa bao lâu spawn thêm 1 con

    //[Header("Skin")]
    //public Material[] botSkins;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Map Triggers")]
    public Transform[] mapTriggerPoints;

    private List<GameObject> wanderingBots = new List<GameObject>();
    private List<GameObject> mapSelectorBots = new List<GameObject>();

    void Start()
    {
        // Spawn wandering bots cố định
        int wanderingCount = Random.Range(minWanderingBots, maxWanderingBots + 1);
        for (int i = 0; i < wanderingCount; i++)
            SpawnWanderingBot();

        // Bắt đầu quản lý MapSelector bots
        StartCoroutine(MapSelectorSpawnLoop());
    }

    // ───── WANDERING ─────
    void SpawnWanderingBot()
    {
        GameObject bot = SpawnBot(SelectMapBot.BotType.Wandering);
        if (bot != null)
            wanderingBots.Add(bot);
    }

    // ───── MAP SELECTOR LOOP ─────
    IEnumerator MapSelectorSpawnLoop()
    {
        while (true)
        {
            // Dọn dẹp list (xóa con đã bị destroy/inactive)
            mapSelectorBots.RemoveAll(b => b == null || !b.activeInHierarchy);

            // Nếu đang thiếu con MapSelector thì spawn thêm
            if (mapSelectorBots.Count < minMapSelectorBots)
            {
                int toSpawn = Random.Range(minMapSelectorBots, maxMapSelectorBots + 1) - mapSelectorBots.Count;
                for (int i = 0; i < toSpawn; i++)
                {
                    yield return new WaitForSeconds(Random.Range(1f, 3f)); // Spawn lệch nhau 1 chút
                    GameObject bot = SpawnBot(SelectMapBot.BotType.MapSelector);
                    if (bot != null)
                        mapSelectorBots.Add(bot);
                }
            }

            // Chờ rồi check lại
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    // ───── SPAWN 1 CON ─────
    GameObject SpawnBot(SelectMapBot.BotType type)
    {
        if (spawnPoints.Length == 0) return null;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
        bot.transform.SetParent(transform);

        //// Random skin
        //if (botSkins.Length > 0)
        //{
        //    Material randomSkin = botSkins[Random.Range(0, botSkins.Length)];
        //    var smr = bot.GetComponentInChildren<SkinnedMeshRenderer>();
        //    if (smr != null) smr.material = randomSkin;
        //}

        // Setup script
        SelectMapBot botScript = bot.GetComponent<SelectMapBot>();
        if (botScript != null)
        {
            botScript.botType = type;
            botScript.mapTriggerPoints = mapTriggerPoints;
            botScript.onDespawn = () => mapSelectorBots.Remove(bot); // Callback khi biến mất
        }

        return bot;
    }
}