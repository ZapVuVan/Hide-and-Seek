// HiderFindHideState.cs
using UnityEngine;
using UnityEngine.AI;

public class HiderFindHideState : IBotState
{
    private HiderRole role;
    private BotStateMachine stateMachine;
    private NavMeshAgent agent;
    private Transform transform;

    private float checkInterval = 1f;
    private float checkTimer = 0f;
    private float timeLimit = 8f;   // tối đa 8s tìm
    private float timeElapsed = 0f;
    private float scoreThreshold = 0.5f; // đủ tốt thì dừng
    private float rayLength = 5f;
    private int rayCount = 8;

    private float bestScoreSoFar = -1f;
    private Vector3 bestPosSoFar = Vector3.zero;

    public HiderFindHideState(HiderRole role, BotStateMachine stateMachine, NavMeshAgent agent, Transform transform)
    {
        this.role = role;
        this.stateMachine = stateMachine;
        this.agent = agent;
        this.transform = transform;
    }

    public void Enter()
    {
        Debug.Log("Hider: Finding hide spot...");
        agent.speed = 12f;
        checkTimer = checkInterval;
        timeElapsed = 0f;
        bestScoreSoFar = -1f;
        bestPosSoFar = Vector3.zero;
        SetRandomWanderDestination();
    }

    public void Exit()
    {
        agent.ResetPath();
    }

    public void Update()
    {
        timeElapsed += Time.deltaTime;
        checkTimer += Time.deltaTime;

        // Hết giờ → đi đến chỗ tốt nhất tìm được rồi trốn
        if (timeElapsed >= timeLimit)
        {
            if (bestPosSoFar != Vector3.zero)
                agent.SetDestination(bestPosSoFar);

            // Đợi đến nơi rồi mới hiding
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                stateMachine.ChangeState(role.HidingState);
            return;
        }

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;

            float score = CalcHideScore(transform.position);

            // Lưu lại chỗ tốt nhất tìm được
            if (score > bestScoreSoFar)
            {
                bestScoreSoFar = score;
                bestPosSoFar = transform.position;
            }

            // Đủ tốt → trốn luôn không cần đợi hết giờ
            if (score >= scoreThreshold)
            {
                Debug.Log("Hider: Good enough spot!");
                agent.ResetPath();
                stateMachine.ChangeState(role.HidingState);
                return;
            }
        }

        // Đến đích → đi tiếp
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            SetRandomWanderDestination();
    }

    private float CalcHideScore(Vector3 position)
    {
        float score = 0f;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 0.5f, dir, out hit, rayLength))
            {
                float proximity = (rayLength - hit.distance) / rayLength;
                score += proximity;
            }
        }

        return score / rayCount; // 0.0 → 1.0
    }

    private void SetRandomWanderDestination()
    {
        Vector3 randomPoint = GetRandomNavMeshPoint(15f);
        if (randomPoint != Vector3.zero)
            agent.SetDestination(randomPoint);
    }

    private Vector3 GetRandomNavMeshPoint(float radius)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir.y = 0;
            randomDir += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
                return hit.position;
        }
        return Vector3.zero;
    }
}