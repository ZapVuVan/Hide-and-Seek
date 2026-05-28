// SpawnManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private List<GameObject> spawnPoints;

    private void Awake() => Instance = this;

    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return Vector3.zero;
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    public void StartRespawn(GameObject obj, float delay)
    {
        StartCoroutine(RespawnCoroutine(obj, delay));
    }

    private IEnumerator RespawnCoroutine(GameObject obj, float delay)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(delay);

        Vector3 spawnPos = GetRandomSpawnPoint();
        obj.transform.position = spawnPos;
        obj.SetActive(true);

        // Đổi sang Seeker
        obj.GetComponent<RoleComponent>()?.SetRole(GameRole.Seeker);
    }
}