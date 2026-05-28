using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHiderState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.hiderCamera.SetActive(true);
        player.hiderControlUI.SetActive(true);
 
    }

    public void ExitState(PlayerController player)
    {
        player.hiderCamera.SetActive(false);
        player.hiderControlUI.SetActive(false);
    }

    public void UpdateState(PlayerController player)
    {
    }
}
