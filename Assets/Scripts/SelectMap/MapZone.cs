using System.Collections.Generic;
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public string sceneName;

    private HashSet<GameObject> playersInside = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInside.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInside.Remove(other.gameObject);
        }
    }

    public int GetPlayerCount()
    {
        return playersInside.Count;
    }
}