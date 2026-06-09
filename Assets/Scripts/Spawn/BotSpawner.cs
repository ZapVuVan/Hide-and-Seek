//using System.Collections.Generic;
//using UnityEngine;

//public class BotSpawner : MonoBehaviour
//{
//    [SerializeField] private GameObject botPrefab;
//    [SerializeField] private int botCount = 5;

//    private void Start()
//    {
//        List<Transform> points =
//            SpawnManager.Instance.GetSpawnPoints(botCount);

//        foreach (Transform p in points)
//        {
//            Instantiate(botPrefab, p.position, p.rotation);
//        }
//    }
//}