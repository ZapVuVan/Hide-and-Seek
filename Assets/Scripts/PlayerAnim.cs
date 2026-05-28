using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement playerMovement;

    private const int BASE_LAYER = 0;
    private const int GUN_LAYER = 1;
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        //anim.SetLayerWeight(GUN_LAYER, 0f);

    }
    private void Update()
    {
        if(playerMovement.GetInputMove() != Vector2.zero)
        {
            anim.SetBool("isRunning", true);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        anim.SetBool("isJumping", playerMovement.IsJumping);
    }

    //public void SetGun(bool isGun)
    //{
    //    anim.SetLayerWeight(GUN_LAYER, isGun ? 1f : 0f);
    //}
}
