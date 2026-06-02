// MapZone.cs - gắn vào BoxCheck_1 và BoxCheck_2
using UnityEngine;

public class MapZone : MonoBehaviour
{
    public string sceneName; // Map_1 hoặc Map_2
    private bool playerInside = false;

    public bool HasPlayer() => playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}