// BotHiderState.cs
using UnityEngine;
using UnityEngine.AI;
public class BotHiderState : IBotState
{
    private InvisibleController invisibleController;
    private enum HiderSubState { Moving, Hiding }
    private HiderSubState subState;
    private float hideRadius = 20f;
    public void EnterState(BotController bot)
    {
        invisibleController = bot.GetComponent<InvisibleController>();
        if (bot.Agent == null) return;

        var outline = bot.GetComponentInChildren<Outline>();
        if (outline != null)
            outline.enabled = false;

        if (GameManager.Instance.CurrentState == GameState.AssigningRoles)
        {
            bot.Agent.isStopped = true;
            return;
        }

        MoveToRandomSpot(bot);
    }
    public void UpdateState(BotController bot)
    {
        if (GameManager.Instance.CurrentState == GameState.AssigningRoles) return;

        if (bot.Agent == null) return;
        float speed = bot.Agent.velocity.magnitude;
        invisibleController?.UpdateInvisible(speed);
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
        invisibleController?.ResetInvisible();
    }
    public void OnHit(BotController bot)
    {
        invisibleController?.ResetInvisible();
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
            Debug.Log($"Bot {bot.name} di chuyển đến vị trí ẩn: {hit.position}");
            bot.Agent.SetDestination(hit.position);
        }
        else
        {
            Debug.Log("Không tìm được điểm trên NavMesh!");
        }
    }
}
