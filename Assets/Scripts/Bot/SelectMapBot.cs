using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SelectMapBot : MonoBehaviour
{
    public enum BotType { Wandering, MapSelector, Random }

    [Header("Bot Settings")]
    public BotType botType = BotType.Random;
    public float wanderRadius = 10f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [Header("Map Selector Settings")]
    public Transform[] mapTriggerPoints;
    public float standAtTriggerMin = 1f;
    public float standAtTriggerMax = 5f;

    [Header("Despawn Settings")]
    public GameObject joinEffect;

    private NavMeshAgent agent;
    private Animator animator;

    public System.Action onDespawn;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        Debug.Log("Agent: " + agent);
        Debug.Log("Animator: " + animator);
        Debug.Log("Agent on NavMesh: " + agent.isOnNavMesh);

        if (botType == BotType.Random)
            botType = Random.value < 0.5f
                ? BotType.Wandering
                : BotType.MapSelector;

        Debug.Log("BotType: " + botType);
        StartCoroutine(BotRoutine());
    }

   

    IEnumerator BotRoutine()
    {
        if (botType == BotType.Wandering)
            yield return StartCoroutine(WanderingRoutine());
        else
            yield return StartCoroutine(MapSelectorRoutine());
    }

    // ───── WANDERING ─────
    IEnumerator WanderingRoutine()
    {
        while (true)
        {
            Vector3 target = GetRandomNavMeshPoint(transform.position, wanderRadius);
            agent.SetDestination(target);

            Debug.Log("Before SetWalkAnim True");
            SetWalkAnim(true);
            Debug.Log("After SetWalkAnim True");

            yield return new WaitUntil(() => HasReached());

            SetWalkAnim(false);
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }
    // ───── MAP SELECTOR ─────
    IEnumerator MapSelectorRoutine()
    {
        while (true)
        {
            // 1. Lang thang vài bước trước
            int preWanders = Random.Range(1, 4);
            for (int i = 0; i < preWanders; i++)
            {
                Vector3 wanderTarget = GetRandomNavMeshPoint(transform.position, wanderRadius);
                agent.SetDestination(wanderTarget);
                SetWalkAnim(true);

                yield return new WaitUntil(() => HasReached());

                SetWalkAnim(false);
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }

            // 2. Đi đến ô trigger
            if (mapTriggerPoints == null || mapTriggerPoints.Length == 0) yield break;
            Transform trigger = mapTriggerPoints[Random.Range(0, mapTriggerPoints.Length)];

            agent.SetDestination(trigger.position);
            SetWalkAnim(true);

            yield return new WaitUntil(() => HasReached());

            // 3. Dừng lại, xoay nhìn vào bảng
            SetWalkAnim(false);
            agent.isStopped = true;
            yield return StartCoroutine(RotateTo(trigger.position));

            // 4. Đứng 1-5s
            float standTime = Random.Range(standAtTriggerMin, standAtTriggerMax);
            yield return new WaitForSeconds(standTime);

            // 5. Biến mất
            yield return StartCoroutine(DespawnEffect());

            // 6. Chờ rồi respawn
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            RespawnAtRandomPoint();
            agent.isStopped = false;
        }
    }

    // ───── HELPERS ─────
    IEnumerator RotateTo(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
            yield return null;
        }
    }

    IEnumerator DespawnEffect()
    {
        if (joinEffect != null)
            Instantiate(joinEffect, transform.position, Quaternion.identity);

        Vector3 originalScale = transform.localScale;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        onDespawn?.Invoke(); // Báo cho Spawner biết con này đã biến mất
        Destroy(gameObject); // Destroy hẳn thay vì SetActive false
        transform.localScale = originalScale;
    }

    void RespawnAtRandomPoint()
    {
        Vector3 spawnPos = GetRandomNavMeshPoint(Vector3.zero, wanderRadius * 2f);
        transform.position = spawnPos;
        gameObject.SetActive(true);
    }

    bool HasReached()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f)
                return true;
        return false;
    }

    Vector3 GetRandomNavMeshPoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = origin + Random.insideUnitSphere * radius;
            randomPoint.y = origin.y;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }

    void SetWalkAnim(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isWalking);
            Debug.Log("Đang set anim: " + isWalking); // Xem trong Console có log ra không
        }
        else
        {
            Debug.LogError("Animator chưa được gán hoặc bị null!");
        }
    }
}