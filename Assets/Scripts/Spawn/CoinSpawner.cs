using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CoinSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int maxCoinsOnMap = 15;
    [SerializeField] private float spawnInterval = 20f;

    [Header("NavMesh Spawn")]
    [SerializeField] private float spawnRadius = 50f;  // bán kính tìm điểm spawn tính từ CoinSpawner
    [SerializeField] private int maxAttempts = 10;     // số lần thử tìm điểm hợp lệ

    private List<GameObject> activeCoins = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnCoin();
        }
    }

    public void TrySpawnCoin()
    {
        activeCoins.RemoveAll(c => c == null);

        if (activeCoins.Count >= maxCoinsOnMap) return;

        Vector3 spawnPos;
        if (!GetRandomNavMeshPoint(out spawnPos)) return;

        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        activeCoins.Add(coin);

        // Hiệu ứng xuất hiện
        coin.transform.localScale = Vector3.zero;
        coin.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    private bool GetRandomNavMeshPoint(out Vector3 result)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            // Lấy điểm random trong vòng tròn bán kính spawnRadius
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = transform.position.y; // giữ nguyên độ cao để tránh spawn trên trời

            NavMeshHit hit;
            // SamplePosition tìm điểm NavMesh gần nhất trong bán kính 5f
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    public void RemoveCoin(GameObject coin)
    {
        activeCoins.Remove(coin);
    }
}