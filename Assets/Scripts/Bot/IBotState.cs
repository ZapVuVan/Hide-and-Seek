public interface IBotState
{
    void EnterState(BotController bot);
    void UpdateState(BotController bot);
    void ExitState(BotController bot);
}