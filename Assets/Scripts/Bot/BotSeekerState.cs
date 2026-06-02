// BotSeekerState.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class BotSeekerState : IBotState
{
    private float patrolRadius = 15f;
    private float shootCooldown = 1.5f;
    private float shootTimer = 0f;
    private float attackRange = 8f;
    private InvisibleController targetInvisible;
    public void EnterState(BotController bot)
    {
        var outline = bot.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.OutlineColor = Color.red;
            outline.enabled = true;
        }
        bot.Agent.isStopped = true;
    }
    public void UpdateState(BotController bot)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            bot.Agent.isStopped = true;
            return;
        }
        shootTimer += Time.deltaTime;
        Transform target = bot.FindNearestHider();
        if (target == null)
        {
            if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                Patrol(bot);
            if (shootTimer >= shootCooldown)
            {
                ShootRandom(bot);
                shootTimer = 0f;
            }
            return;
        }
        if (targetInvisible == null || targetInvisible.transform != target)
            targetInvisible = target.GetComponent<InvisibleController>();
        bool isFullyInvisible = targetInvisible != null && targetInvisible._fillAmount >= 1f;
        if (!isFullyInvisible)
        {
            float dist = Vector3.Distance(bot.transform.position, target.position);
            if (dist > attackRange)
            {
                bot.Agent.isStopped = false;
                bot.Agent.stoppingDistance = attackRange;
                bot.Agent.SetDestination(target.position);
            }
            else
            {
                bot.Agent.isStopped = true;
                Vector3 dir = (target.position - bot.transform.position).normalized;
                bot.transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
                if (shootTimer >= shootCooldown)
                {
                    ShootAt(bot, target.position);
                    shootTimer = 0f;
                }
            }
        }
        else
        {
            bot.Agent.isStopped = false;
            bot.Agent.stoppingDistance = 0.5f;
            if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                Patrol(bot);
            if (shootTimer >= shootCooldown)
            {
                ShootRandom(bot);
                shootTimer = 0f;
            }
        }
    }
    public void ExitState(BotController bot)
    {
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;
        var outline = bot.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }
    private void Patrol(BotController bot)
    {
        Vector3 randomPos = bot.transform.position + Random.insideUnitSphere * patrolRadius;
        randomPos.y = bot.transform.position.y;
        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            bot.Agent.SetDestination(hit.position);
    }
    private void ShootAt(BotController bot, Vector3 targetPos)
    {
        bot.GetComponent<BotShoot>()?.Shoot(targetPos);
    }
    private void ShootRandom(BotController bot)
    {
        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y = 0;
        bot.GetComponent<BotShoot>()?.Shoot(bot.transform.position + randomDir * 10f);
    }
}