using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float airMultiplier = 0.4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;

    private Rigidbody rb;
    private bool grounded;
    private bool isJumping = false;
    private bool readyToJump = true;
    private Vector2 inputMove;
    private PlayerController playerController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        inputMove = InputManager.instance.GetMoveInput();

        if (grounded && isJumping)
            isJumping = false;
        if (InputManager.instance.GetJumpInput() && readyToJump && grounded && !isJumping)
        {
            readyToJump = false;
            isJumping = true;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void FixedUpdate()
    {
        Vector3 flatForward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 flatRight = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        Vector3 moveDir = flatForward * inputMove.y + flatRight * inputMove.x;

        if (grounded)
        {
            rb.velocity = new Vector3(
                moveDir.normalized.x * moveSpeed,
                rb.velocity.y,
                moveDir.normalized.z * moveSpeed
            );
        }
        else
        {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limited = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
            }
        }
        if (playerController.IsFirstPerson())
        {
            Vector3 rot = orientation.eulerAngles;

            playerObj.rotation =
                Quaternion.Euler(0f, rot.y, 0f);
        }
        if (moveDir != Vector3.zero && !playerController.IsFirstPerson())
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            playerObj.rotation = Quaternion.Slerp(
                playerObj.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    

    private void ResetJump() => readyToJump = true;
    public float GetSpeed() => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

    public Vector2 GetInputMove() => inputMove;
    public bool IsJumping => isJumping;

    private void OnCollisionStay(Collision collision)
    {
        // Check layer
        if (((1 << collision.gameObject.layer) & whatIsGround) != 0)
            grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsGround) != 0)
            grounded = false;
    }

    public void ApplySpeedBoost(float amount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(amount, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
    }
}