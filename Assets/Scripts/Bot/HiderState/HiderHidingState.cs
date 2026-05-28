using UnityEngine;

public class HiderHidingState : IBotState
{
    private HiderRole role;
    private BotStateMachine stateMachine;

    public HiderHidingState(HiderRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Hider: Hiding...");
    }

    public void Exit() { }

    public void Update()
    {
        // TODO: check Seeker trong tầm nhìn (FOV / raycast)
        // Seeker phát hiện → chạy ngay
        // if (IsSeekerNearby()) stateMachine.ChangeState(role.FleeState);
    }
}
