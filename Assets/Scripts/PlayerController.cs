using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private IPlayerState currentState;
    public PlayerHiderState playerHiderState = new PlayerHiderState();
    public PlayerSeekerState playerSeekerState = new PlayerSeekerState();
    public PlayerNormalState playerNormalState = new PlayerNormalState();
    [HideInInspector] public PlayerMovement movement;


    public GameObject hiderCamera;
    public GameObject seekerCamera;

    public GameObject hiderControlUI;
    public GameObject seekerControlUI;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();

    }

    private void Start()
    {
        TransitionToState(playerNormalState);
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void TransitionToState(IPlayerState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }

        currentState = newState;
        currentState.EnterState(this);
    }

    public IPlayerState GetCurrentState() => currentState;

    public bool IsFirstPerson()
    {
        return currentState == playerSeekerState;
    }


}
