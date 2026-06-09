//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerAnimSM : MonoBehaviour
//{
//    private Animator anim;
//    private PlayerMovementSM playerMovement;
//    private PlayerController playerController;
//    private const int BASE_LAYER = 0;
//    private const int GUN_LAYER = 1;
//    private void Awake()
//    {
//        anim = GetComponentInChildren<Animator>();
//        playerMovement = GetComponent<PlayerMovementSM>();

//    }
//    private void Update()
//    {
//        if (playerMovement.GetInputMove() != Vector2.zero)
//        {
//            anim.SetBool("isRunning", true);
//        }
//        else
//        {
//            anim.SetBool("isRunning", false);
//        }

//        anim.SetBool("isJumping", playerMovement.IsJumping);
//    }

//}
