using UnityEngine;

public class HiderFleeState : IBotState
{
    private HiderRole role;
    private BotStateMachine stateMachine;

    public HiderFleeState(HiderRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Hider: Flee!");
    }

    public void Exit() { }

    public void Update()
    {
        // TODO: chạy theo hướng ngược Seeker
        // RunAwayFromSeeker();

        // Thoát đủ xa → tìm chỗ trốn mới
        // if (IsSafeDistance()) stateMachine.ChangeState(role.FindHideState);
    }
}
