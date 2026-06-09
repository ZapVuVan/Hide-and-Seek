using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Bot Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private List<Transform> availablePoints;

    private void Awake()
    {
        Instance = this;
        ResetPool();
    }

    public void ResetPool()
    {
        availablePoints = new List<Transform>();

        if (spawnPoints == null) return;

        foreach (var p in spawnPoints)
        {
            if (p != null)
                availablePoints.Add(p);
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        if (availablePoints == null || availablePoints.Count == 0)
            ResetPool();

        if (availablePoints.Count == 0)
        {
            Debug.LogError("No spawn points available!");
            return null;
        }

        int index = Random.Range(0, availablePoints.Count);
        Transform point = availablePoints[index];

        availablePoints.RemoveAt(index);

        return point;
    }

    public List<Transform> GetSpawnPoints(int count)
    {
        ResetPool();

        List<Transform> result = new List<Transform>();

        int safeCount = Mathf.Min(count, spawnPoints.Length);

        for (int i = 0; i < safeCount; i++)
        {
            result.Add(GetRandomSpawnPoint());
        }

        return result;
    }


}