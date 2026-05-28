// BotController.cs
using System;
using UnityEngine;
using UnityEngine.AI;

public class BotController : MonoBehaviour, IRole
{
    [HideInInspector] public NavMeshAgent Agent;

    private IBotState currentState;
    public BotNormalState normalState = new BotNormalState();
    public BotHiderState hiderState = new BotHiderState();
    public BotSeekerState seekerState = new BotSeekerState();

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        GetComponent<Health>().OnDie += HandleDie;
    }

    private void HandleDie()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        GetComponent<RoleComponent>().SetRole(GameRole.None);
    }

    private void Update()

    {
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
    public void OnHit()
    {
        if (currentState is BotHiderState hiderState)
            hiderState.OnHit(this);
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
}