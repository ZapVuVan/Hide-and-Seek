using UnityEngine;

public class SeekerChaseState : IBotState
{
    private SeekerRole role;
    private BotStateMachine stateMachine;

    public SeekerChaseState(SeekerRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Seeker: Chase!");
    }

    public void Exit() { }

    public void Update()
    {
        // TODO: chạy theo vị trí Hider
        // ChaseTarget();

        // Mất dấu → tìm kiếm
        // if (LostTarget()) stateMachine.ChangeState(role.SearchState);

        // Bắt được → kết thúc
        // if (CaughtTarget()) { /* xử lý bắt được */ }
    }
}
