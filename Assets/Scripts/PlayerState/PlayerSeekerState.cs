using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSeekerState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.seekerCamera.SetActive(true);
        player.seekerControlUI.SetActive(true);
    }

    public void ExitState(PlayerController player)
    {
        player.seekerCamera.SetActive(false);
        player.seekerControlUI.SetActive(false);
    }

    public void UpdateState(PlayerController player)
    {
       
    }
}
