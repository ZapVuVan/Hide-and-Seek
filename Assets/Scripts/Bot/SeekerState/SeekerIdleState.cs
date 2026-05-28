using UnityEngine;

public class SeekerIdleState : IBotState
{
    private SeekerRole role;
    private BotStateMachine stateMachine;

    public SeekerIdleState(SeekerRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Seeker: Idle");
    }

    public void Exit() { }

    public void Update()
    {
        // Tự động chuyển sang tuần tra
        stateMachine.ChangeState(role.PatrolState);
    }
}
