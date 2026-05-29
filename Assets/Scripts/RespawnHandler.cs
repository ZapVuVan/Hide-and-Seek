// RespawnHandler.cs
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 2f;

    private Health health;
    private RoleComponent roleComponent;

    private void Awake()
    {
        health = GetComponent<Health>();
        roleComponent = GetComponent<RoleComponent>();
        health.OnDie += HandleDie;
    }

    private void HandleDie()
    {
        if (roleComponent.Role != GameRole.Hider) return;
        SpawnManager.Instance.StartRespawn(gameObject, respawnDelay);
    }
}