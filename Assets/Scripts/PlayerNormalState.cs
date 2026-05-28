using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        player.seekerCamera.SetActive(false);
        player.seekerControlUI.SetActive(false);
        player.hiderCamera.SetActive(true);
        player.hiderControlUI.SetActive(false);
    }

    public void ExitState(PlayerController player)
    {
    }

    public void UpdateState(PlayerController player)
    {
    }


}
