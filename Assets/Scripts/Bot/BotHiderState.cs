// BotHiderState.cs
using UnityEngine;
using UnityEngine.AI;

public class BotHiderState : IBotState
{
    private enum HiderSubState { Moving, Hiding }
    private HiderSubState subState;
    private float hideRadius = 20f;

    public void EnterState(BotController bot)
    {
        if (bot.Agent == null) return;
        MoveToRandomSpot(bot);
    }

    public void UpdateState(BotController bot)
    {
        if (bot.Agent == null) return;
        float speed = bot.Agent.velocity.magnitude;
        bot.GetComponent<InvisibleController>()?.UpdateInvisible(speed);

        switch (subState)
        {
            case HiderSubState.Moving:
                if (bot.Agent.pathPending) break;
                if (bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                {
                    bot.Agent.isStopped = true;
                    subState = HiderSubState.Hiding;
                }
                break;

            case HiderSubState.Hiding:
                break;
        }
    }

    public void ExitState(BotController bot)
    {
        if (bot.Agent == null) return;
        bot.Agent.isStopped = false;
        bot.GetComponent<InvisibleController>()?.ResetInvisible();
    }

    public void OnHit(BotController bot)
    {
        bot.GetComponent<InvisibleController>()?.ResetInvisible();
        MoveToRandomSpot(bot);
    }

    private void MoveToRandomSpot(BotController bot)
    {
        subState = HiderSubState.Moving;
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;

        Vector3 randomPos = bot.transform.position + Random.insideUnitSphere * hideRadius;
        randomPos.y = bot.transform.position.y;

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            Debug.Log($"Di chuyển đến: {hit.position}");
            bot.Agent.SetDestination(hit.position);
        }
        else
        {
            Debug.Log("Không tìm được điểm trên NavMesh!");
        }
    }
}