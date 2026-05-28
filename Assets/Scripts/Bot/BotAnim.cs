// BotAnim.cs
using UnityEngine;
using UnityEngine.AI;

public class BotAnim : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent agent;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {

        anim.SetBool("isRunning", agent.velocity.magnitude > 0.1f);
    }
}