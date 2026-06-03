using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSeekerState : IPlayerState
{
    public void EnterState(PlayerController player)
    {
        Debug.Log("EnterState Seeker");
        player.headMeshRenderer.SetActive(false);
        player.seekerCamera.gameObject.SetActive(true);
        player.seekerControlUI.SetActive(true);
        player.GetComponent<InvisibleController>()?.ResetInvisible();
            //var outline = player.GetComponent<Outline>();
            //if (outline != null)
            //{
            //    outline.OutlineColor = Color.red;
            //    outline.enabled = true;
            //}
    }

    public void ExitState(PlayerController player)
    {
        player.headMeshRenderer.SetActive(true);
        player.seekerCamera.gameObject.SetActive(false);
        player.seekerControlUI.SetActive(false);
        //var outline = player.GetComponent<Outline>();
        //if (outline != null) outline.enabled = false;
    }

    public void UpdateState(PlayerController player)
    {
       
    }
}
