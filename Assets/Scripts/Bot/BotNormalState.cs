
public class BotNormalState : IBotState
{
    public void EnterState(BotController bot)
    {
        bot.Agent.isStopped = true;
    }

    public void UpdateState(BotController bot) { }

    public void ExitState(BotController bot)
    {
        bot.Agent.isStopped = false;
    }
}