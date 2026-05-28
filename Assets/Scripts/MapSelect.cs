using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MapZoneData
{
    public BoxCollider zone;
    public string mapName;
}

public class MapSelect : MonoBehaviour
{
    [SerializeField] private float timeSelectMax = 1f;
    public MapZoneData[] mapZones;
    private float timeSelect = 0;
    private bool hasSelected = false;

    private void Update()
    {
        if (!hasSelected)
        {
            timeSelect += Time.deltaTime;

            if (timeSelect >= timeSelectMax)
            {
                hasSelected = true;
                SelectMap();
            }
        }
    }

    void SelectMap()
    {
        int[] playerCounts = new int[mapZones.Length];

        for (int i = 0; i < mapZones.Length; i++)
        {
            Collider[] objectsInZone = Physics.OverlapBox(
                mapZones[i].zone.bounds.center,
                mapZones[i].zone.bounds.extents,
                mapZones[i].zone.transform.rotation
            );

          
            foreach (var obj in objectsInZone)
            {
                if (obj.CompareTag("Player"))
                {
                    playerCounts[i]++;
                   
                }
            }

  
        }

        int max = Mathf.Max(playerCounts);

        List<MapZoneData> candidates = new List<MapZoneData>();
        for (int i = 0; i < playerCounts.Length; i++)
        {
            if (playerCounts[i] == max)
                candidates.Add(mapZones[i]);
        }

        MapZoneData selected = candidates[Random.Range(0, candidates.Count)];

        SceneManager.LoadScene(selected.mapName);
    }
  
}