using UnityEngine;
using UnityEngine.AI;

public class BotSeekerState : IBotState
{
    // =====================
    // CONFIG
    // =====================
    private float maxShootRange = 30f;
    private float horizontalFOV = 140f;
    private float verticalFOV = 80f;
    private float shootCooldown = 0.35f;
    private float accuracy = 0.45f;
    private float missOffset = 2.5f;
    private float shootStopDistance = 12f;
    private float pingReachThreshold = 50f;
    private float searchDuration = 8f;
    private float patrolShootInterval = 1.8f;

    // Cơ chế phản xạ Ping & Chống Hack Wall
    private float pingReactionDelay = 0.6f;
    private float currentPingDelayTimer = 0f;
    private float wallPingErrorRadius = 5.0f;

    private float nextShootTime = 0f;
    private float nextPatrolShootTime = 0f;
    private Transform currentTarget = null;
    private Vector3 lastKnownPosition;
    private float lostTargetTimer = 0f;
    private float lostTargetDuration = 2f;
    private bool returningToPatrol = true;

    private enum PingState { None, GoToPing, ShootAtPing, Search }
    private PingState pingState = PingState.None;
    private Vector3 pingPoint;
    private float searchTimer = 0f;

    // Patrol Search (Waypoints)
    private bool patrolSearching = false;
    private float patrolSearchTimer = 0f;
    private float patrolSearchDuration = 3f;
    private float patrolSearchRadius = 8f;

    // Trạng thái đồng bộ Warning UI cá nhân của từng con Bot
    private bool isCurrentlyTargetingPlayerUI = false;

    public void EnterState(BotController bot)
    {
        if (bot.Agent == null || !bot.Agent.enabled || !bot.Agent.isOnNavMesh) return;
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = shootStopDistance;

        nextShootTime = Time.time + shootCooldown;
        nextPatrolShootTime = Time.time + patrolShootInterval;

        pingState = PingState.None;
        returningToPatrol = true;
        patrolSearching = false;
        currentPingDelayTimer = 0f;
        isCurrentlyTargetingPlayerUI = false;
    }

    public void ExitState(BotController bot)
    {
        bot.Agent.isStopped = false;
        bot.Agent.stoppingDistance = 0.5f;
        currentTarget = null;
        bot.PingTarget = null;
        bot.PingActive = false;
        pingState = PingState.None;

        if (isCurrentlyTargetingPlayerUI)
        {
            DetectionWarningUI.Instance?.UpdateTargetState(false);
            isCurrentlyTargetingPlayerUI = false;
        }
    }

    public void UpdateState(BotController bot)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            bot.Agent.isStopped = true;
            return;
        }

        // XỬ LÝ NHẬN PING
        if (bot.PingTarget.HasValue)
        {
            Vector3 rawPingPoint = bot.PingTarget.Value;
            bot.PingTarget = null;

            Vector3 eyePos = bot.transform.position + Vector3.up * 1.5f;
            Vector3 targetDirection = rawPingPoint - eyePos;
            float targetDist = targetDirection.magnitude;

            if (Physics.Raycast(eyePos, targetDirection.normalized, out RaycastHit hit, targetDist))
            {
                if (!hit.transform.CompareTag("Player"))
                {
                    Vector2 randomOffset = Random.insideUnitCircle * wallPingErrorRadius;
                    Vector3 fuzzyPoint = hit.point + new Vector3(randomOffset.x, 0f, randomOffset.y);

                    if (NavMesh.SamplePosition(fuzzyPoint, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                    {
                        pingPoint = navHit.position;
                    }
                    else
                    {
                        pingPoint = hit.point;
                    }
                }
                else
                {
                    pingPoint = rawPingPoint;
                }
            }
            else
            {
                pingPoint = rawPingPoint;
            }

            float navDist = bot.GetNavMeshDistance(pingPoint);
            pingState = navDist <= pingReachThreshold ? PingState.ShootAtPing : PingState.GoToPing;

            patrolSearching = false;
            returningToPatrol = false;
            currentPingDelayTimer = 0f;
        }

        // Tìm target trong tầm nhìn FOV thực tế (Kiểm tra sống/chết và khoảng cách)
        Transform visibleTarget = FindVisibleTarget(bot);

        if (visibleTarget != null)
        {
            currentTarget = visibleTarget;
            lastKnownPosition = visibleTarget.position;
            lostTargetTimer = lostTargetDuration;
        }
        else
        {
            if (currentTarget != null)
            {
                Health targetHealth = currentTarget.GetComponent<Health>();
                if (targetHealth != null && targetHealth.currentHealth <= 0)
                {
                    currentTarget = null;
                    lostTargetTimer = 0f;
                }
            }

            if (lostTargetTimer > 0f)
                lostTargetTimer -= Time.deltaTime;
            else
                currentTarget = null;
        }

        // XỬ LÝ ĐỒNG BỘ WARNING UI QUA BỘ ĐẾM TARGET COUNTER
        bool shouldTargetPlayerUI = (currentTarget != null && currentTarget.CompareTag("Player")) ||
                                    pingState == PingState.GoToPing ||
                                    pingState == PingState.ShootAtPing;

        if (shouldTargetPlayerUI != isCurrentlyTargetingPlayerUI)
        {
            isCurrentlyTargetingPlayerUI = shouldTargetPlayerUI;
            DetectionWarningUI.Instance?.UpdateTargetState(isCurrentlyTargetingPlayerUI);
        }

        // Xử lý di chuyển và hành động dựa theo trạng thái AI hiện tại
        if (currentTarget != null)
        {
            patrolSearching = false;
            pingState = PingState.None;
            HandleChase(bot);
            return;
        }

        if (lostTargetTimer > 0f)
        {
            HandleLostTarget(bot);
            return;
        }

        if (pingState != PingState.None)
        {
            HandlePing(bot);
            return;
        }

        Patrol(bot);
    }

    private void HandleLostTarget(BotController bot)
    {
        RotateToTarget(bot, lastKnownPosition);
        bot.Agent.stoppingDistance = 1f;
        float dist = Vector3.Distance(bot.transform.position, lastKnownPosition);

        if (dist <= 2f)
        {
            if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance + 0.1f)
            {
                Vector3 randomPos = lastKnownPosition + Random.insideUnitSphere * 6f;
                randomPos.y = lastKnownPosition.y;

                if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    bot.Agent.isStopped = false;
                    bot.Agent.SetDestination(hit.position);
                }
            }
        }
        else
        {
            bot.Agent.isStopped = false;
            bot.Agent.SetDestination(lastKnownPosition);
        }
    }

    private void HandleChase(BotController bot)
    {
        RotateToTarget(bot, currentTarget.position);
        float dist = Vector3.Distance(bot.transform.position, currentTarget.position);

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
        if (currentPingDelayTimer > 0f)
        {
            currentPingDelayTimer -= Time.deltaTime;
        }

        switch (pingState)
        {
            case PingState.ShootAtPing:
                RotateToTarget(bot, pingPoint);
                bot.Agent.isStopped = true;

                if (currentPingDelayTimer <= 0f)
                {
                    TryShoot(bot, pingPoint);
                }

                if (!bot.PingActive)
                {
                    pingState = PingState.Search;
                    searchTimer = searchDuration;
                }
                break;

            case PingState.GoToPing:
                bot.Agent.isStopped = false;
                bot.Agent.stoppingDistance = 2f;
                bot.Agent.SetDestination(pingPoint);
                RotateToTarget(bot, pingPoint);

                if (currentPingDelayTimer <= 0f)
                {
                    TryShoot(bot, pingPoint);
                }

                bool arrivedAtPing = !bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance;
                if (!bot.PingActive || arrivedAtPing)
                {
                    pingState = PingState.Search;
                    searchTimer = searchDuration;
                }
                break;

            case PingState.Search:
                searchTimer -= Time.deltaTime;
                RotateToTarget(bot, pingPoint);

                if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance + 0.1f)
                {
                    Vector3 randomPos = pingPoint + (Vector3)(Random.insideUnitCircle * 8f);
                    randomPos.y = pingPoint.y;

                    if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        bot.Agent.isStopped = false;
                        bot.Agent.stoppingDistance = 1f;
                        bot.Agent.SetDestination(hit.position);
                    }
                }

                if (searchTimer <= 0f)
                {
                    pingState = PingState.None;
                    SetNearestWaypointToPing(bot);
                    returningToPatrol = true;

                    bot.Agent.isStopped = false;
                    bot.Agent.stoppingDistance = 1f;
                    bot.Agent.SetDestination(bot.patrolWaypoints[bot.currentWaypointIndex].position);
                }
                break;
        }
    }

    private Transform FindVisibleTarget(BotController bot)
    {
        var hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);
        if (hiders == null || hiders.Count == 0) return null;

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hider in hiders)
        {
            if (hider == null) continue;

            Health targetHealth = hider.GetComponent<Health>();
            if (targetHealth != null && targetHealth.currentHealth <= 0) continue;

            Transform t = hider.transform;
            Vector3 dir = t.position - bot.transform.position;
            float dist = dir.magnitude;

            if (dist > maxShootRange) continue;

            Vector3 localDir = bot.transform.InverseTransformDirection(dir.normalized);
            float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;

            if (Mathf.Abs(horizontalAngle) > horizontalFOV * 0.5f) continue;
            if (Mathf.Abs(verticalAngle) > verticalFOV * 0.5f) continue;

            Vector3 eyePos = bot.transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = t.position + Vector3.up * 1f;

            if (Physics.Raycast(eyePos, (targetPos - eyePos).normalized, out RaycastHit hit, dist))
            {
                if (!hit.transform.IsChildOf(t) && hit.transform != t)
                {
                    continue;
                }
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
        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        bot.transform.rotation = Quaternion.Slerp(bot.transform.rotation, targetRot, Time.deltaTime * 15f);
    }

    private void TryShoot(BotController bot, Vector3 aimPoint)
    {
        if (Time.time < nextShootTime) return;

        Vector3 fireOrigin = bot.transform.position + Vector3.up * 1.5f;
        Vector3 finalAimPoint = aimPoint + Vector3.up * 1.2f;

        bool isHit = Random.value <= accuracy;

        if ((currentTarget == null && pingState != PingState.None) || !isHit)
        {
            float dynamicMiss = (currentTarget == null) ? missOffset * 2.2f : missOffset;
            Vector2 horizontalMiss = Random.insideUnitCircle * dynamicMiss;

            float dice = Random.value;
            float verticalMiss = 0f;
            float distanceModifier = 1.0f;

            if (dice < 0.2f)
            {
                verticalMiss = Random.Range(3.0f, 6.0f);
                distanceModifier = Random.Range(0.8f, 1.3f);
            }
            else if (dice < 0.5f)
            {
                verticalMiss = Random.Range(-0.4f, 0f);
                distanceModifier = 0.75f;
            }
            else
            {
                verticalMiss = Random.Range(0.2f, 1.8f);
                distanceModifier = 1.25f;
            }

            Vector3 targetDir = (finalAimPoint - fireOrigin).normalized;
            float currentDist = Vector3.Distance(fireOrigin, finalAimPoint);

            finalAimPoint = fireOrigin + targetDir * (currentDist * distanceModifier);
            finalAimPoint += new Vector3(horizontalMiss.x, verticalMiss, horizontalMiss.y);
        }

        Vector3 finalDirection = (finalAimPoint - fireOrigin).normalized;
        Vector3 bulletDestinationFarAway = fireOrigin + finalDirection * 500f;

        bot.GetComponent<BotShoot>()?.Shoot(bulletDestinationFarAway);
        nextShootTime = Time.time + shootCooldown;
    }

    private void ShootPatrolSweep(BotController bot)
    {
        if (Time.time < nextPatrolShootTime) return;

        Vector3 fireOrigin = bot.transform.position + Vector3.up * 1.5f;
        Vector3 baseDir = bot.Agent.velocity.sqrMagnitude > 0.1f ? bot.Agent.velocity.normalized : bot.transform.forward;

        float randomAngle = Random.Range(-35f, 35f);
        Vector3 sweepDir = Quaternion.Euler(0, randomAngle, 0) * baseDir;

        float patrolShootRange = Random.Range(8f, 18f);
        Vector3 shootTarget = fireOrigin + sweepDir * patrolShootRange;

        float randomHeight = Random.value < 0.25f ? Random.Range(2.5f, 4.5f) : Random.Range(0.8f, 1.5f);
        Vector3 finalPatrolTarget = new Vector3(shootTarget.x, bot.transform.position.y + randomHeight, shootTarget.z);

        Vector3 sweepDirection = (finalPatrolTarget - fireOrigin).normalized;
        Vector3 patrolDestinationFarAway = fireOrigin + sweepDirection * 500f;

        bot.GetComponent<BotShoot>()?.Shoot(patrolDestinationFarAway);
        nextPatrolShootTime = Time.time + patrolShootInterval;
    }

    private void SetNearestWaypointToPing(BotController bot)
    {
        if (bot.patrolWaypoints == null || bot.patrolWaypoints.Length == 0) return;

        float minDist = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < bot.patrolWaypoints.Length; i++)
        {
            float dist = Vector3.Distance(pingPoint, bot.patrolWaypoints[i].position);
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
        ShootPatrolSweep(bot);
        patrolSearchTimer -= Time.deltaTime;

        if (patrolSearchTimer <= 0f)
        {
            patrolSearching = false;
            bot.currentWaypointIndex = (bot.currentWaypointIndex + 1) % bot.patrolWaypoints.Length;
            bot.Agent.isStopped = false;
            bot.Agent.stoppingDistance = 1f;
            bot.Agent.SetDestination(bot.patrolWaypoints[bot.currentWaypointIndex].position);
            return;
        }

        if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance + 0.1f)
        {
            Vector3 center = bot.patrolWaypoints[bot.currentWaypointIndex].position;
            Vector3 randomPos = center + Random.insideUnitSphere * patrolSearchRadius;
            randomPos.y = center.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                bot.Agent.isStopped = false;
                bot.Agent.stoppingDistance = 1f;
                bot.Agent.SetDestination(hit.position);
            }
        }
    }

    private void Patrol(BotController bot)
    {
        ShootPatrolSweep(bot);

        if (patrolSearching)
        {
            PatrolSearch(bot);
            return;
        }

        if (bot.patrolWaypoints == null || bot.patrolWaypoints.Length == 0)
        {
            bot.Agent.isStopped = false;
            bot.Agent.stoppingDistance = 1f;

            if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance + 0.1f)
            {
                Vector3 randomPos = bot.transform.position + Random.insideUnitSphere * 15f;
                randomPos.y = bot.transform.position.y;

                if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    bot.Agent.SetDestination(hit.position);
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
                float dist = Vector3.Distance(bot.transform.position, bot.patrolWaypoints[i].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIndex = i;
                }
            }

            bot.currentWaypointIndex = nearestIndex;
            bot.Agent.SetDestination(bot.patrolWaypoints[bot.currentWaypointIndex].position);
            returningToPatrol = false;
        }
        else if (!bot.Agent.pathPending && bot.Agent.remainingDistance <= bot.Agent.stoppingDistance + 0.1f)
        {
            patrolSearching = true;
            patrolSearchTimer = patrolSearchDuration;
        }
    }
}