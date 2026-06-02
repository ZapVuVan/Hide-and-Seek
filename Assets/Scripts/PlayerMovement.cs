using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float maxFallSpeed = -35f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;

    private CharacterController controller;
    private PlayerController playerController;

    private Vector2 inputMove;
    private float verticalVelocity;
    private bool grounded;
    private bool isJumping;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        inputMove = InputManager.instance.GetMoveInput();
        grounded = controller.isGrounded;

        HandleJump();
        Move();
        RotatePlayer();
    }

    private void HandleJump()
    {
        if (grounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            isJumping = false;
        }

        if (InputManager.instance.GetJumpInput() && grounded)
        {
            isJumping = true;

            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Move()
    {
        Vector3 flatForward =
            new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;

        Vector3 flatRight =
            new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;

        Vector3 moveDir =
            flatForward * inputMove.y +
            flatRight * inputMove.x;

        Vector3 horizontalVelocity = moveDir.normalized * moveSpeed;

        // GRAVITY (mượt nhưng không float)
        if (verticalVelocity < 0)
        {
            verticalVelocity += gravity * 1.8f * Time.deltaTime;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        Vector3 flatForward =
            new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;

        Vector3 flatRight =
            new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;

        Vector3 moveDir =
            flatForward * inputMove.y +
            flatRight * inputMove.x;

        if (playerController.IsFirstPerson())
        {
            playerObj.rotation =
                Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);
        }
        else if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);

            playerObj.rotation = Quaternion.Slerp(
                playerObj.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }
    public void ApplySpeedBoost(float amount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(amount, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        moveSpeed = moveSpeed + amount;

        yield return new WaitForSeconds(duration);

        moveSpeed = moveSpeed;
    }
    // ===== Optional debug helpers =====

    public float GetSpeed()
    {
        Vector3 v = controller.velocity;
        v.y = 0;
        return v.magnitude;
    }
    public Vector2 GetInputMove()
    {
        return inputMove;
    }
    public bool IsJumping => isJumping;
}