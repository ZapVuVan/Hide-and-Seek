// BotSetup.cs - gắn lên bot GameObject
using UnityEngine;
using UnityEngine.AI;

public class BotSetup : MonoBehaviour
{
    private BotController botController;
    private NavMeshAgent agent;

    private void Start()
    {
        botController = GetComponent<BotController>();
        agent = GetComponent<NavMeshAgent>();

        // Tìm player
        Transform player = GameObject.FindWithTag("Player").transform;

        // Gán role Hider
        botController.ChangeRole(new HiderRole(botController, agent, transform, player));
    }
}