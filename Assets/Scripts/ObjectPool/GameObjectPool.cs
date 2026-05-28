// GameObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public static GameObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject Get(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool '{tag}' không tồn tại!");
            return null;
        }

        GameObject obj = poolDictionary[tag].Count > 0
            ? poolDictionary[tag].Dequeue()
            : Instantiate(pools.Find(p => p.tag == tag).prefab, transform);

        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public void Return(string tag, GameObject obj)
    {
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}