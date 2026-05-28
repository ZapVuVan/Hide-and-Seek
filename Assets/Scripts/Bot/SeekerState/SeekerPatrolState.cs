using UnityEngine;

public class SeekerPatrolState : IBotState
{
    private SeekerRole role;
    private BotStateMachine stateMachine;

    public SeekerPatrolState(SeekerRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Seeker: Patrolling...");
    }

    public void Exit() { }

    public void Update()
    {
        // TODO: di chuyển theo waypoints
        // MoveToNextWaypoint();

        // Nhìn thấy Hider → đuổi theo
        // if (CanSeeHider()) stateMachine.ChangeState(role.ChaseState);
    }
}
