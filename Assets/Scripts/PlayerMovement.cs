using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ladder Climb")]
    [SerializeField] private float climbSpeed = 4f;
    private bool isClimbing = false;
    private Collider currentLadderCollider;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -35f;
    [SerializeField] private float maxFallSpeed = -50f;
    [SerializeField] private float fallMultiplier = 2.8f;

    [Header("Auto Jump")]
    [SerializeField] private float autoJumpDistance = 1.0f;
    [SerializeField] private float autoJumpMaxHeight = 1.2f;
    [SerializeField] private float autoJumpCooldown = 0.25f;

    [Header("Air Control")]
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float airAcceleration = 4f;

    [Header("Layer Setup")]
    [Tooltip("Tích chọn các Layer chướng ngại vật (vách, tường) để tự nhảy. Tuyệt đối KHÔNG chọn layer của Thang.")]
    [SerializeField] private LayerMask whatIsObstacle;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;

    private CharacterController controller;
    private PlayerController playerController;

    private Vector2 inputMove;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    private bool grounded;
    private bool isJumping;
    private float lastAutoJumpTime;
    private bool isFrozen = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 1. KIỂM TRA TRẠNG THÁI CHẾT
        if (playerController != null && playerController.IsDead)
        {
            inputMove = Vector2.zero;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = grounded ? -5f : verticalVelocity + gravity * Time.deltaTime;

            Vector3 deadMove = horizontalVelocity;
            deadMove.y = verticalVelocity;
            controller.Move(deadMove * Time.deltaTime);
            return;
        }

        // 2. KIỂM TRA TRẠNG THÁI ĐÓNG BĂNG
        if (isFrozen)
        {
            inputMove = Vector2.zero;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = grounded ? -5f : verticalVelocity + gravity * Time.deltaTime;

            Vector3 freezeMove = horizontalVelocity;
            freezeMove.y = verticalVelocity;
            controller.Move(freezeMove * Time.deltaTime);
            return;
        }

        inputMove = InputManager.instance.GetMoveInput();
        grounded = controller.isGrounded;

        // 3. XỬ LÝ DI CHUYỂN PHÂN TÁCH BIỆT RÕ RÀNG TRẠNG THÁI
        if (isClimbing)
        {
            HandleClimbMove();
        }
        else
        {
            HandleJump();
            CheckAutoJump();
            Move();
        }

        RotatePlayer();
    }

    // Thiết lập trạng thái leo thang (Được gọi từ LadderTrigger ngoài Map)
    public void SetClimbing(bool climbing, Collider ladderCollider = null)
    {
        isClimbing = climbing;
        currentLadderCollider = ladderCollider;

        if (isClimbing)
        {
            isJumping = false;
            verticalVelocity = 0f;
            horizontalVelocity = Vector3.zero;
        }
    }

    // ✅ FIX: Logic leo thang đã được cải thiện
    private void HandleClimbMove()
    {
        // ✅ FIX 1: Triệt tiêu gravity LIÊN TỤC mỗi frame — không để gravity re-apply
        // Trick của Code Monkey: ép grounded = true và verticalVelocity = 0 mỗi frame
        verticalVelocity = 0f;
        grounded = true;

        float climbVertical = inputMove.y * climbSpeed;

        Vector3 right = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        Vector3 climbHorizontal = right * inputMove.x * (moveSpeed * 0.5f);

        Vector3 finalClimbVelocity = climbHorizontal;
        finalClimbVelocity.y = climbVertical;

        controller.Move(finalClimbVelocity * Time.deltaTime);

        // ✅ FIX 3: Xử lý lên đỉnh thang — dùng Coroutine thay vì đẩy 1 frame (tránh giật)
        if (currentLadderCollider != null)
        {
            float ladderTopY = currentLadderCollider.bounds.max.y;

            // Mở rộng threshold lên 0.3f để detect sớm hơn, tránh bị kẹt mép
            if (transform.position.y > (ladderTopY - 0.3f) && inputMove.y > 0.1f)
            {
                SetClimbing(false, null);
                StartCoroutine(StepOverLadderTop());
                return;
            }
        }

        // Tự động tháo bám thang nếu trèo xuống thấp và chân chạm đất
        if (grounded && inputMove.y < -0.1f)
        {
            SetClimbing(false, null);
        }

        // Nhảy bung ra khỏi thang
        if (InputManager.instance.GetJumpInput())
        {
            SetClimbing(false, null);
            DoJump();
        }
    }

    // ✅ FIX 3: Coroutine đẩy player qua mép thang trong 0.15s — mượt, không giật
    private IEnumerator StepOverLadderTop()
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 forwardStep = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;

        while (elapsed < duration)
        {
            controller.Move((forwardStep * moveSpeed * 0.8f + Vector3.up * 2f) * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void SetFreeze(bool freeze)
    {
        isFrozen = freeze;
        if (isFrozen)
        {
            inputMove = Vector2.zero;
            horizontalVelocity = Vector3.zero;
        }
    }

    private void HandleJump()
    {
        if (grounded && verticalVelocity < 0)
        {
            verticalVelocity = -5f;
            isJumping = false;
        }

        if (InputManager.instance.GetJumpInput() && grounded)
        {
            DoJump();
        }
    }

    private void DoJump()
    {
        isJumping = true;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        lastAutoJumpTime = Time.time;
    }

    // ✅ FIX 2: CheckAutoJump không còn trigger nhầm khi đứng gần thang
    private void CheckAutoJump()
    {
        if (!grounded || isJumping || isClimbing) return;

        // ✅ Nếu đang trong vùng trigger của thang thì bỏ qua hoàn toàn
        if (currentLadderCollider != null) return;

        // ✅ Cooldown check (bản gốc bị thiếu phần này)
        if (Time.time - lastAutoJumpTime < autoJumpCooldown) return;

        Vector3 forward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Debug.DrawRay(origin, forward * autoJumpDistance, Color.red);

        if (!Physics.Raycast(origin, forward, out RaycastHit hit, autoJumpDistance, whatIsObstacle))
            return;

        if (hit.collider.CompareTag("Ladder")) return;

        if (inputMove.y > 0.1f)
        {
            DoJump();
        }
    }

    private void Move()
    {
        Vector3 forward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 right = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;

        Vector3 moveDir = forward * inputMove.y + right * inputMove.x;

        float accel = grounded ? acceleration : airAcceleration;
        float control = grounded ? 1f : airControlMultiplier;

        Vector3 targetVelocity = moveDir * moveSpeed * control;

        horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, accel * Time.deltaTime);

        if (verticalVelocity < 0)
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        else
            verticalVelocity += gravity * Time.deltaTime;

        verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void RotatePlayer()
    {
        if (isClimbing) return;

        Vector3 forward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 right = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;

        Vector3 moveDir = forward * inputMove.y + right * inputMove.x;

        if (playerController.IsFirstPerson())
        {
            playerObj.rotation = Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);
        }
        else if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplySpeedBoost(float amount, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(amount, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || orientation == null) return;

        Vector3 forward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + forward * autoJumpDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin + forward * autoJumpDistance, 0.2f);
    }

    public float GetSpeed()
    {
        Vector3 v = controller.velocity;
        v.y = 0;
        return v.magnitude;
    }

    public Vector2 GetInputMove() => inputMove;
    public bool IsJumping => isJumping;
    public bool IsClimbing => isClimbing;
}