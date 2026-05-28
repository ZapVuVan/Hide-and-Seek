using UnityEngine;

public class SeekerSearchState : IBotState
{
    private SeekerRole role;
    private BotStateMachine stateMachine;

    public SeekerSearchState(SeekerRole role, BotStateMachine stateMachine)
    {
        this.role         = role;
        this.stateMachine = stateMachine;
    }

    public void Enter()
    {
        Debug.Log("Seeker: Searching...");
    }

    public void Exit() { }

    public void Update()
    {
        // TODO: tìm kiếm quanh vị trí cuối thấy Hider
        // SearchLastKnownPosition();

        // Tìm thấy lại → đuổi
        // if (CanSeeHider()) stateMachine.ChangeState(role.ChaseState);

        // Tìm không ra → về Patrol
        // if (SearchTimeout()) stateMachine.ChangeState(role.PatrolState);
    }
}
