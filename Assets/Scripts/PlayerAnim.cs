using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement playerMovement;
    private PlayerController playerController;
    private RoleComponent role;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerController = GetComponent<PlayerController>();
        role = GetComponent<RoleComponent>();
    }

    private void Update()
    {
        if (playerController == null || anim == null) return;

        anim.SetBool("isDead", playerController.IsDead);

        if (playerController.IsDead) return;

        // ĐỒNG BỘ ANIMATION LEO THANG
        anim.SetBool("isClimbing", playerMovement.IsClimbing);

        // Nếu đang trèo thang, có thể đóng băng tốc độ phát Anim khi đứng im trên thang
        if (playerMovement.IsClimbing)
        {
            // Nếu có di chuyển (bấm joystick) thì anim chạy tốc độ 1, đứng im thì anim dừng (tốc độ 0)
            anim.speed = (playerMovement.GetInputMove() != Vector2.zero) ? 1f : 0f;

            anim.SetBool("isRunning", false);
            anim.SetBool("isJumping", false);
            return;
        }
        else
        {
            anim.speed = 1f; // Trả lại tốc độ anim bình thường khi rời thang
        }

        // Logic Chạy cũ
        if (playerMovement.GetInputMove() != Vector2.zero)
        {
            anim.SetBool("isRunning", true);
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        anim.SetBool("isJumping", playerMovement.IsJumping);
        anim.SetBool("isSeeker", role.Role == GameRole.Seeker);
    }
}