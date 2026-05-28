// HiderRole.cs
using UnityEngine;
using UnityEngine.AI;

public class HiderRole : IBotRole
{
    private BotController bot;
    private BotStateMachine stateMachine;
    private NavMeshAgent agent;
    private Transform transform;
    private Transform playerTransform;

    public float NormalSpeed = 3.5f;
    public float FleeSpeed = 6f;

    public HiderFindHideState FindHideState;
    public HiderHidingState HidingState;
    public HiderFleeState FleeState;

    // Sửa constructor nhận đủ 4 tham số
    public HiderRole(BotController bot, NavMeshAgent agent, Transform transform, Transform playerTransform)
    {
        this.bot = bot;
        this.agent = agent;
        this.transform = transform;
        this.playerTransform = playerTransform;

        stateMachine = new BotStateMachine();

        FindHideState = new HiderFindHideState(this, stateMachine, agent, transform);
        //HidingState = new HiderHidingState(this, stateMachine, agent);
        //FleeState = new HiderFleeState(this, stateMachine, agent, transform, playerTransform);
    }

    public void Enter()
    {
        Debug.Log("Role: Hider");
        agent.speed = NormalSpeed;
        stateMachine.ChangeState(FindHideState);
    }

    public void Update() { stateMachine.Update(); }

    public void Exit() { Debug.Log("Exit Hider Role"); }

    public void OnHit() { stateMachine.ChangeState(FleeState); }
}