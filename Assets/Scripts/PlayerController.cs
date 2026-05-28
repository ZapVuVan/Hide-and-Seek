// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour, IRole
{
    [HideInInspector] public PlayerMovement movement;

    public GameObject hiderCamera;
    public GameObject seekerCamera;
    public GameObject hiderControlUI;
    public GameObject seekerControlUI;

    private IPlayerState currentState;
    public PlayerNormalState normalState = new PlayerNormalState();
    public PlayerHiderState hiderState = new PlayerHiderState();
    public PlayerSeekerState seekerState = new PlayerSeekerState();

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        GetComponent<RoleComponent>().SetRole(GameRole.None);
        TransitionToState(normalState);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void TransitionToState(IPlayerState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState?.EnterState(this);
    }

    public IPlayerState GetCurrentState() => currentState;
    public bool IsFirstPerson() => currentState == seekerState;

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
}