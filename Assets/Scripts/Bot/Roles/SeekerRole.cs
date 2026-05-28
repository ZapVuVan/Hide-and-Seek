using UnityEngine;

public class SeekerRole : IBotRole
{
    private BotController bot;
    private BotStateMachine stateMachine;

    public SeekerIdleState   IdleState;
    public SeekerPatrolState PatrolState;
    public SeekerChaseState  ChaseState;
    public SeekerSearchState SearchState;

    public SeekerRole(BotController bot)
    {
        this.bot     = bot;
        stateMachine = new BotStateMachine();

        IdleState   = new SeekerIdleState(this, stateMachine);
        PatrolState = new SeekerPatrolState(this, stateMachine);
        ChaseState  = new SeekerChaseState(this, stateMachine);
        SearchState = new SeekerSearchState(this, stateMachine);
    }

    public void Enter()
    {
        Debug.Log("Role: Seeker");
        stateMachine.ChangeState(PatrolState);
    }

    public void Update()
    {
        stateMachine.Update();
    }

    public void Exit()
    {
        Debug.Log("Exit Seeker Role");
    }
}
