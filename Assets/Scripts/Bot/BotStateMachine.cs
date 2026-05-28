public class BotStateMachine
{
    private IBotState currentState;

    public void ChangeState(IBotState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
