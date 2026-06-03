using UnityEngine;
using UnityEngine.AI;

public class BotSeekerState : IBotState
{
    // =====================
    // CONFIG
    // =====================
    private float maxShootRange = 50f;
    private float fieldOfView = 120f;
    private float shootCooldown = 0.4f;
    private float accuracy = 0.55f;
    private float missOffset = 2f;
    private float shootStopDistance = 20f;
    private float pingReachThreshold = 35f;
    private float searchDuration = 5f;

    private float nextShootTime = 0f;
    private Transform currentTarget = null;
    private InvisibleController targetInvisible = null;
    private bool returningToPatrol = true;

    private enum PingState { None, GoToPing, ShootAtPing, Search }
    private PingState pingState = PingState.None;
    private Vector3 pingPoint;
    private float searchTimer = 0f;

    // Patrol Search
    private bool patrolSearching = false;
    private float patrolSearchTimer = 0f;
    private float patrolSearchDuration = 3f;
    private float patrolSearchRadius = 8f;

    public void EnterState(BotController bot)
    {
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = shootStopDistance;

        nextShootTime = Time.time + shootCooldown;

        pingState = PingState.None;
        returningToPatrol = true;
        patrolSearching = false;
    }

    public void ExitState(BotController bot)
    {
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;

        currentTarget = null;

        bot.PingTarget = null;
        bot.PingActive = false;

        pingState = PingState.None;

        DetectionWarningUI.Instance?.SetDetected(false);
    }

    public void UpdateState(BotController bot)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            bot.Agent.isStopped = true;
            DetectionWarningUI.Instance?.SetDetected(false);
            return;
        }

        if (bot.PingTarget.HasValue && pingState == PingState.None)
        {
            pingPoint = bot.PingTarget.Value;
            bot.PingTarget = null;

            float navDist = bot.GetNavMeshDistance(pingPoint);

            pingState = navDist <= pingReachThreshold
                ? PingState.ShootAtPing
                : PingState.GoToPing;
        }

        currentTarget = FindVisibleTarget(bot);

        bool isDetected =
            (currentTarget != null && currentTarget.CompareTag("Player")) ||
            pingState == PingState.GoToPing ||
            pingState == PingState.ShootAtPing;

        DetectionWarningUI.Instance?.SetDetected(isDetected);

        // Priority 1
        if (currentTarget != null)
        {
            patrolSearching = false;
            pingState = PingState.None;

            HandleChase(bot);
            return;
        }

        // Priority 2
        if (pingState != PingState.None)
        {
            HandlePing(bot);
            return;
        }

        // Priority 3
        Patrol(bot);
    }

    private void HandleChase(BotController bot)
    {
        RotateToTarget(bot, currentTarget.position);

        float dist = Vector3.Distance(
            bot.transform.position,
            currentTarget.position);

        if (dist > shootStopDistance)
        {
            bot.Agent.isStopped = false;
            bot.Agent.stoppingDistance = shootStopDistance;
            bot.Agent.SetDestination(currentTarget.position);
        }
        else
        {
            bot.Agent.isStopped = true;
        }

        TryShoot(bot, currentTarget.position);
    }

    private void HandlePing(BotController bot)
    {
        switch (pingState)
        {
            case PingState.ShootAtPing:

                RotateToTarget(bot, pingPoint);

                bot.Agent.isStopped = true;

                TryShoot(bot, pingPoint);

                if (!bot.PingActive)
                {
                    pingState = PingState.Search;
                    searchTimer = searchDuration;
                }
                break;

            case PingState.GoToPing:

                bot.Agent.isStopped = false;
                bot.Agent.stoppingDistance = shootStopDistance;
                bot.Agent.SetDestination(pingPoint);

                float distToPoint =
                    Vector3.Distance(bot.transform.position, pingPoint);

                if (distToPoint <= shootStopDistance)
                {
                    RotateToTarget(bot, pingPoint);
                    TryShoot(bot, pingPoint);
                }

                if (!bot.PingActive ||
                    (!bot.Agent.pathPending &&
                     bot.Agent.remainingDistance <= bot.Agent.stoppingDistance))
                {
                    pingState = PingState.Search;
                    searchTimer = searchDuration;
                }

                break;

            case PingState.Search:

                searchTimer -= Time.deltaTime;

                if (searchTimer <= 0f)
                {
                    pingState = PingState.None;

                    SetNearestWaypointToPing(bot);

                    patrolSearching = true;
                    patrolSearchTimer = patrolSearchDuration;

                    bot.Agent.SetDestination(
                        bot.patrolWaypoints[bot.currentWaypointIndex].position);

                    break;
                }

                if (!bot.Agent.pathPending &&
                    bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
                {
                    Vector3 randomPos =
                        pingPoint +
                        (Vector3)(Random.insideUnitCircle * 8f);

                    randomPos.y = pingPoint.y;

                    if (NavMesh.SamplePosition(
                        randomPos,
                        out NavMeshHit hit,
                        5f,
                        NavMesh.AllAreas))
                    {
                        bot.Agent.isStopped = false;
                        bot.Agent.stoppingDistance = 1f;
                        bot.Agent.SetDestination(hit.position);
                    }
                }
                break;
        }
    }

    private Transform FindVisibleTarget(BotController bot)
    {
        var hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);

        if (hiders == null || hiders.Count == 0)
            return null;

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hider in hiders)
        {
            if (hider == null) continue;

            Transform t = hider.transform;

            var invisible = t.GetComponent<InvisibleController>();
            if (invisible != null && invisible._fillAmount >= 0.7f)
                continue;

            Vector3 dir = t.position - bot.transform.position;
            float dist = dir.magnitude;

            if (dist > maxShootRange)
                continue;

            float angle =
                Vector3.Angle(bot.transform.forward, dir);

            if (angle > fieldOfView / 2f)
                continue;

            Vector3 eyePos =
                bot.transform.position + Vector3.up * 1.5f;

            Vector3 targetPos =
                t.position + Vector3.up * 1f;

            if (Physics.Raycast(
                eyePos,
                (targetPos - eyePos).normalized,
                out RaycastHit hit,
                dist))
            {
                if (!hit.transform.IsChildOf(t) &&
                    hit.transform != t)
                    continue;
            }

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = t;
            }
        }

        return nearest;
    }

    private void RotateToTarget(BotController bot, Vector3 targetPos)
    {
        Vector3 dir = targetPos - bot.transform.position;

        dir.y = 0;

        if (dir == Vector3.zero)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        bot.transform.rotation = Quaternion.Slerp(
            bot.transform.rotation,
            targetRot,
            Time.deltaTime * 15f);
    }

    private void TryShoot(BotController bot, Vector3 aimPoint)
    {
        if (Time.time < nextShootTime)
            return;

        bool isHit = Random.value <= accuracy;

        if (!isHit)
        {
            aimPoint += Random.insideUnitSphere * missOffset;
        }

        bot.GetComponent<BotShoot>()?.Shoot(
            aimPoint + Vector3.up * 1.2f);

        nextShootTime = Time.time + shootCooldown;
    }

    private void SetNearestWaypointToPing(BotController bot)
    {
        if (bot.patrolWaypoints == null ||
            bot.patrolWaypoints.Length == 0)
            return;

        float minDist = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < bot.patrolWaypoints.Length; i++)
        {
            float dist = Vector3.Distance(
                pingPoint,
                bot.patrolWaypoints[i].position);

            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        bot.currentWaypointIndex = nearestIndex;
    }

    private void PatrolSearch(BotController bot)
    {
        patrolSearchTimer -= Time.deltaTime;

        if (patrolSearchTimer <= 0f)
        {
            patrolSearching = false;

            bot.currentWaypointIndex =
                (bot.currentWaypointIndex + 1) %
                bot.patrolWaypoints.Length;

            bot.Agent.SetDestination(
                bot.patrolWaypoints[bot.currentWaypointIndex].position);

            return;
        }

        if (!bot.Agent.pathPending &&
            bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
        {
            Vector3 center =
                bot.patrolWaypoints[bot.currentWaypointIndex].position;

            Vector3 randomPos =
                center +
                Random.insideUnitSphere * patrolSearchRadius;

            randomPos.y = center.y;

            if (NavMesh.SamplePosition(
                randomPos,
                out NavMeshHit hit,
                5f,
                NavMesh.AllAreas))
            {
                bot.Agent.SetDestination(hit.position);
            }
        }
    }

    private void Patrol(BotController bot)
    {
        if (patrolSearching)
        {
            PatrolSearch(bot);
            return;
        }

        if (bot.patrolWaypoints == null ||
            bot.patrolWaypoints.Length == 0)
        {
            bot.Agent.isStopped = false;
            bot.Agent.stoppingDistance = 1f;

            if (!bot.Agent.pathPending &&
                bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
            {
                Vector3 randomPos =
                    bot.transform.position +
                    Random.insideUnitSphere * 15f;

                randomPos.y = bot.transform.position.y;

                if (NavMesh.SamplePosition(
                    randomPos,
                    out NavMeshHit hit,
                    5f,
                    NavMesh.AllAreas))
                {
                    bot.Agent.SetDestination(hit.position);
                }
            }

            return;
        }

        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 1f;

        if (returningToPatrol)
        {
            float minDist = float.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < bot.patrolWaypoints.Length; i++)
            {
                float dist = Vector3.Distance(
                    bot.transform.position,
                    bot.patrolWaypoints[i].position);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIndex = i;
                }
            }

            bot.currentWaypointIndex = nearestIndex;

            bot.Agent.SetDestination(
                bot.patrolWaypoints[bot.currentWaypointIndex].position);

            returningToPatrol = false;
        }
        else if (!bot.Agent.pathPending &&
                 bot.Agent.remainingDistance <= bot.Agent.stoppingDistance)
        {
            patrolSearching = true;
            patrolSearchTimer = patrolSearchDuration;
        }
    }
}
