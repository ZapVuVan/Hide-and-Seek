using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour, IRole
{
    [SerializeField] public Transform[] patrolWaypoints;
    [HideInInspector] public int currentWaypointIndex = 0;
    [HideInInspector] public NavMeshAgent Agent;
    private IBotState currentState;
    public BotNormalState normalState = new BotNormalState();
    public BotHiderState hiderState = new BotHiderState();
    public BotSeekerState seekerState = new BotSeekerState();
    private bool isFrozen;
    private float freezeTimer;

    // =====================
    // PING
    // =====================
    public Vector3? PingTarget { get; set; } = null;
    public bool PingActive { get; set; } = false;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        GetComponent<Health>().OnDie += HandleDie;
        var killTable = FindObjectOfType<KillTable>();
        if (killTable != null)
            GetComponent<Health>().OnKilled += killTable.OnKilled;
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPingHiders += OnPingReceived;
        GameManager.Instance.OnPingEnd += OnPingEnd;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPingHiders -= OnPingReceived;
        GameManager.Instance.OnPingEnd -= OnPingEnd;
    }

    private void OnPingReceived(List<RoleComponent> hiders)
    {
        if (GetComponent<RoleComponent>().Role != GameRole.Seeker) return;
        if (hiders == null || hiders.Count == 0) return;

        RoleComponent nearest = null;
        float minDist = float.MaxValue;

        foreach (var h in hiders)
        {
            if (h == null) continue;
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; nearest = h; }
        }

        if (nearest != null)
        {
            PingTarget = nearest.transform.position;
            PingActive = true;
        }
    }

    private void OnPingEnd()
    {
        PingActive = false;
    }

    public float GetNavMeshDistance(Vector3 targetPos)
    {
        NavMeshPath path = new NavMeshPath();
        if (Agent.CalculatePath(targetPos, path) &&
            path.status == NavMeshPathStatus.PathComplete)
        {
            float dist = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                dist += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return dist;
        }
        return float.MaxValue;
    }

    private void HandleDie()
    {
        PingTarget = null;
        PingActive = false;
        gameObject.SetActive(false);
    }

    private void Start()
    {
        GetComponent<RoleComponent>().SetRole(GameRole.None);
    }

    private void Update()
    {
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
            }
            if (freezeTimer <= 0f)
                isFrozen = false;
            return;
        }
        currentState?.UpdateState(this);
    }

    public void TransitionToState(IBotState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public void OnRoleChanged(GameRole role)
    {
        switch (role)
        {
            case GameRole.Hider:
                TransitionToState(hiderState);
                break;
            case GameRole.Seeker:
                TransitionToState(seekerState);
                break;
            default:
                TransitionToState(normalState);
                break;
        }
    }

    public void ApplyFreeze(float duration)
    {
        isFrozen = true;
        freezeTimer = duration;
        if (Agent != null)
            Agent.isStopped = true;
    }

    public Transform FindHideSpot()
    {
        var spots = GameObject.FindGameObjectsWithTag("HideSpot");
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var spot in spots)
        {
            float dist = Vector3.Distance(transform.position, spot.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = spot.transform;
            }
        }
        return nearest;
    }

    public Transform FindNearestHider()
    {
        var hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var hider in hiders)
        {
            float dist = Vector3.Distance(transform.position, hider.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hider.transform;
            }
        }
        return nearest;
    }

    public void OnHit()
    {
        if (currentState is BotHiderState hiderState)
            hiderState.OnHit(this);
    }
}