using UnityEngine;

public class HiderIdleState : IBotState
{
    private HiderRole role;
    private BotStateMachine stateMachine;

    public HiderIdleState(HiderRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Hider: Idle");
    }

    public void Exit() { }

    public void Update()
    {
        stateMachine.ChangeState(role.FindHideState);
    }
}
