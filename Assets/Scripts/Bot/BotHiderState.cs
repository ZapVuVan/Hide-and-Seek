using UnityEngine;
using UnityEngine.AI;

public class BotHiderState : IBotState
{
    private InvisibleController invisibleController;

    private enum HiderSubState
    {
        Idle,
        Moving,
        Hiding
    }

    private HiderSubState subState;

    private float hideRadius = 20f;
    private GameState lastGameState;

    public void EnterState(BotController bot)
    {
        if (bot.Agent == null) return;

        invisibleController = bot.GetComponent<InvisibleController>();

        //var outline = bot.GetComponentInChildren<Outline>();
        //if (outline != null)
        //    outline.enabled = false;

        subState = HiderSubState.Idle;

        lastGameState = GameManager.Instance.CurrentState;
    }

    public void UpdateState(BotController bot)
    {
        if (bot.Agent == null) return;

        GameState currentState = GameManager.Instance.CurrentState;

        // =========================
        // 1. AssigningRoles → đứng yên
        // =========================
        if (currentState == GameState.AssigningRoles)
        {
            bot.Agent.isStopped = true;
            return;
        }

        // =========================
        // 2. Vừa chuyển sang HidingPhase → bắt đầu chạy
        // =========================
        if (lastGameState == GameState.AssigningRoles &&
            currentState == GameState.HidingPhase)
        {
            StartHiding(bot);
        }

        lastGameState = currentState;

        // =========================
        // 3. Update invisible theo tốc độ
        // =========================
        float speed = bot.Agent.velocity.magnitude;
        invisibleController?.UpdateInvisible(speed);

        // =========================
        // 4. State logic
        // =========================
        switch (subState)
        {
            case HiderSubState.Moving:

                bot.Agent.isStopped = false;

                if (!bot.Agent.pathPending &&
                    bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                {
                    bot.Agent.isStopped = true;
                    subState = HiderSubState.Hiding;
                }
                break;

            case HiderSubState.Hiding:
                // đứng yên + tăng invisible nếu cần
                bot.Agent.isStopped = true;
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
        StartHiding(bot);
    }

    private void StartHiding(BotController bot)
    {
        subState = HiderSubState.Moving;

        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;

        Vector3 randomPos = bot.transform.position + Random.insideUnitSphere * hideRadius;
        randomPos.y = bot.transform.position.y;

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            bot.Agent.SetDestination(hit.position);
            Debug.Log($"Bot {bot.name} đang đi trốn tới {hit.position}");
        }
        else
        {
            Debug.LogWarning("Không tìm được điểm trên NavMesh!");
        }
    }
}