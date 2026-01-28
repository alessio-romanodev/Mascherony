using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;
    [SerializeField] private float airControlMultiplier = 1f; // controllo totale in aria

    [Header("Jump")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Jump / Slide")]
    [SerializeField] private float wallJumpForceX = 7f;
    [SerializeField] private float wallJumpForceY = 7f;
    [SerializeField] private float wallJumpCoyoteTime = 0.12f;
    [SerializeField] private float wallJumpLockTime = 0.08f;
    [SerializeField] private float wallSlideSpeed = 7.5f; // MOLTO VELOCE

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 0.3f;

    [Header("Gravity (Ultra Snappy)")]
    [SerializeField] private float fallGravityMultiplier = 4.2f;
    [SerializeField] private float jumpCutGravityMultiplier = 3f;
    [SerializeField] private float maxFallSpeed = 30f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler input;

    private bool isGrounded;
    private bool jumpBuffered;
    private bool canJump = true;

    // WALL
    private int wallDir;
    private float lastOnWallTime;
    private float wallJumpTimer;
    private bool isWallJumping;
    private bool isWallSliding;

    // DASH
    private bool isDashing;
    private bool canDash = true;
    private float dashTimer;
    private float dashCooldownTimer;
    private int dashDirection = 1;

    private Collider currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        input = GetComponent<PlayerInputHandler>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (input.JumpPressed)
            jumpBuffered = true;

        if (input.DashPressed && canDash && !isDashing && dashCooldownTimer <= 0f)
            StartDash();

        lastOnWallTime -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleWallDetection();
        HandleDash();
        HandleWallSlide();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    // ================= MOVEMENT =================
    private void HandleMovement()
    {
        if (isDashing) return;

        Vector3 velocity = rb.linearVelocity;
        float control = isGrounded ? 1f : airControlMultiplier;

        if (isWallJumping)
        {
            wallJumpTimer -= Time.fixedDeltaTime;
            control *= 0.75f;

            if (wallJumpTimer <= 0f)
                isWallJumping = false;
        }

        velocity.x = Mathf.Lerp(
            velocity.x,
            input.MoveInput * moveSpeed,
            control
        );

        rb.linearVelocity = velocity;
    }

    // ================= JUMP =================
    private void HandleJump()
    {
        if (!jumpBuffered) return;

        if (isGrounded && input.DropDown)
        {
            DropThroughPlatform();
            jumpBuffered = false;
            return;
        }

        // WALL JUMP
        if (!isGrounded && lastOnWallTime > 0f)
        {
            jumpBuffered = false;

            rb.linearVelocity = new Vector3(
                -wallDir * wallJumpForceX,
                wallJumpForceY,
                0f
            );

            isWallJumping = true;
            wallJumpTimer = wallJumpLockTime;
            lastOnWallTime = 0f;
            wallDir = 0;
            return;
        }

        // NORMAL JUMP
        if (!isGrounded || !canJump) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        jumpBuffered = false;
        canJump = false;
    }

    // ================= WALL SLIDE =================
    private void HandleWallSlide()
    {
        isWallSliding =
            !isGrounded &&
            wallDir != 0 &&
            rb.linearVelocity.y < 0f &&
            !isDashing;

        if (isWallSliding)
        {
            // NON annulliamo X, NON blocchiamo input
            Vector3 velocity = rb.linearVelocity;
            velocity.y = -wallSlideSpeed;
            rb.linearVelocity = velocity;
        }
    }

    // ================= DASH =================
    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (Mathf.Abs(input.MoveInput) > 0.1f)
            dashDirection = input.MoveInput > 0 ? 1 : -1;

        rb.linearVelocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);
    }

    private void HandleDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);

        if (dashTimer <= 0f)
            isDashing = false;
    }

    // ================= GRAVITY =================
    private void ApplyGravity()
    {
        if (isDashing) return;

        Vector3 velocity = rb.linearVelocity;

        if (velocity.y > 0f && !input.JumpHeld)
        {
            velocity.y += Physics.gravity.y * jumpCutGravityMultiplier * Time.fixedDeltaTime;
        }
        else if (velocity.y < 0f)
        {
            velocity.y += Physics.gravity.y * fallGravityMultiplier * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        }

        rb.linearVelocity = velocity;
    }

    // ================= DETECTION =================
    private void CheckGround()
    {
        float rayLength = capsule.height / 2f + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        RaycastHit hit;
        bool wasGrounded = isGrounded;

        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength, groundLayer))
        {
            isGrounded = true;
            canJump = true;
            canDash = true;
            currentPlatform = hit.collider.CompareTag("Platform") ? hit.collider : null;
        }
        else
        {
            isGrounded = false;
            currentPlatform = null;
        }

        if (!wasGrounded && isGrounded)
            jumpBuffered = false;
    }

    private void HandleWallDetection()
    {
        float rayLength = capsule.radius + 0.2f;
        Vector3 center = transform.position;

        bool hitRight = Physics.Raycast(center, Vector3.right, rayLength);
        bool hitLeft = Physics.Raycast(center, Vector3.left, rayLength);

        if (hitRight)
        {
            wallDir = 1;
            lastOnWallTime = wallJumpCoyoteTime;
        }
        else if (hitLeft)
        {
            wallDir = -1;
            lastOnWallTime = wallJumpCoyoteTime;
        }
        else
        {
            wallDir = 0;
        }
    }

    // ================= PLATFORM DROP =================
    private void DropThroughPlatform()
    {
        if (currentPlatform == null) return;
        StartCoroutine(DropCoroutine(currentPlatform));
    }

    private IEnumerator DropCoroutine(Collider platform)
    {
        Physics.IgnoreCollision(platform, capsule, true);
        yield return new WaitForSeconds(0.2f);
        if (platform != null)
            Physics.IgnoreCollision(platform, capsule, false);
    }
}
