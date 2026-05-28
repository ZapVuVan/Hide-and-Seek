using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSeekerState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        Debug.Log("EnterState Seeker");
        player.seekerCamera.gameObject.SetActive(true);
        player.seekerControlUI.SetActive(true);
        player.GetComponent<InvisibleController>()?.ResetInvisible();
        var outline = player.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void ExitState(PlayerController player)
    {
        player.seekerCamera.gameObject.SetActive(false);
        player.seekerControlUI.SetActive(false);
    }

    public void UpdateState(PlayerController player)
    {
       
    }
}
