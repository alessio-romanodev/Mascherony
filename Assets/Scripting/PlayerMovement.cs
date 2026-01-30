using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    public Transform lastCheckpoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8.5f;
    [SerializeField] private float airControlMultiplier = 1f;
    public bool canMove = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask oneWayPlatformLayer;
    [SerializeField] private float jumpCoyoteTime = 0.12f;
    public float lastOnGroundTime;

    [Header("Jump Control")]
    [SerializeField] private float maxJumpHoldTime = 0.15f;
    private float jumpHoldTimer;
    public bool canBufferJump = true;

    [Header("Wall Jump / Slide")]
    [SerializeField] private float wallJumpForceX = 7f;
    [SerializeField] private float wallJumpForceY = 8.2f;
    [SerializeField] private float wallJumpCoyoteTime = 0.12f;
    [SerializeField] private float wallJumpLockTime = 0.08f;
    [SerializeField] private float wallSlideSpeed = 7.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 0.3f;
    private int facingDirection = 1;

    [Header("DashVisual")]
    [SerializeField] private GameObject dashSprite;
    [SerializeField] private float dashPositionModifier;
    [SerializeField] private SpriteRenderer dashSpriteRenderer;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 4.2f;
    [SerializeField] private float jumpCutGravityMultiplier = 3f;
    [SerializeField] private float maxFallSpeed = 30f;

    [Header("Attack")]
    [SerializeField] private Transform attackTransform;
    [SerializeField] private float attackPositionModifier;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler input;

    public bool isGrounded;
    private bool jumpBuffered;
    public bool canJump = true;

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

    //Animator
    public Vector3 Velocity => rb.linearVelocity;
    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public bool IsWallSliding => isWallSliding;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        input = GetComponent<PlayerInputHandler>();
        rb.freezeRotation = true;
        if (dashSprite != null)
            dashSprite.SetActive(false);
    }

    private void Update()
    {
        if (canMove)
        {
            if (input.JumpPressed)
                jumpBuffered = true;

            if (input.DashPressed && canDash && !isDashing && dashCooldownTimer <= 0f)
                StartDash();

            lastOnWallTime -= Time.deltaTime;
            dashCooldownTimer -= Time.deltaTime;
        }


        // CONTINUO CHECK DEL TERRENO PER EVITARE IL BLOCCO DEL SALTO
        CheckGround();
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            HandleWallDetection();
            HandleDash();
            HandleWallSlide();
            HandleMovement();
            HandleJump();
            ApplyGravity();
            UpdateAttackTransformPosition();
        }

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

        float targetX = input.MoveInput * moveSpeed;
        if (input.MoveInput > 0.1f)
            facingDirection = 1;
        else if (input.MoveInput < -0.1f)
            facingDirection = -1;

        if (isWallSliding && Mathf.Sign(input.MoveInput) == wallDir)
            targetX = 0f;

        velocity.x = Mathf.Lerp(velocity.x, targetX, control);
        rb.linearVelocity = velocity;
    }

    private void UpdateAttackTransformPosition()
    {
        if (attackTransform == null) return;

        float dir = 0f;

        if (input.MoveInput > 0.1f)
        {
            dir = 1f;
            dashSpriteRenderer.flipX = false;
        }
        else if (input.MoveInput < -0.1f)
        {
            dir = -1f;
            dashSpriteRenderer.flipX = true;

        }
        else return;

        attackTransform.localPosition = new Vector3(
            dir * Mathf.Abs(attackPositionModifier),
            attackTransform.localPosition.y,
            attackTransform.localPosition.z
        );

        if (dashSprite != null)
        {
            dashSprite.transform.localPosition = new Vector3(
                dir * -Mathf.Abs(dashPositionModifier),
                dashSprite.transform.localPosition.y,
                dashSprite.transform.localPosition.z
            );
        }
    }

    // ================= JUMP =================
    private void HandleJump()
    {
        // WALL JUMP
        if (!isGrounded && lastOnWallTime > 0f && jumpBuffered)
        {
            jumpBuffered = false;
            canBufferJump = false;

            rb.linearVelocity = new Vector3(
                -wallDir * wallJumpForceX,
                wallJumpForceY,
                0f
            );

            jumpHoldTimer = maxJumpHoldTime;
            isWallJumping = true;
            wallJumpTimer = wallJumpLockTime;
            lastOnWallTime = 0f;
            wallDir = 0;
            AudioManager.Instance.PlayJump();

            return;
        }

        // NORMAL JUMP con coyote time
        if (jumpBuffered && (isGrounded || lastOnGroundTime > 0f) && canJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
            jumpHoldTimer = maxJumpHoldTime;

            jumpBuffered = false;
            canJump = false;
            canBufferJump = false;
            lastOnGroundTime = 0f;
            AudioManager.Instance.PlayJump();

        }
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
        if (dashSprite != null)
            dashSprite.SetActive(true);

        if (Mathf.Abs(input.MoveInput) > 0.1f)
            dashDirection = input.MoveInput > 0 ? 1 : -1;
        else
            dashDirection = facingDirection;

        rb.linearVelocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);
        AudioManager.Instance.PlayDash();

    }

    private void HandleDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(dashDirection * dashSpeed, 0f, 0f);

        if (dashTimer <= 0f)
        {
            isDashing = false;
            if (dashSprite != null)
                dashSprite.SetActive(false);
        }
    }

    // ================= GRAVITY =================
    private void ApplyGravity()
    {
        if (isDashing) return;

        Vector3 velocity = rb.linearVelocity;

        if (velocity.y > 0f)
        {
            if (input.JumpHeld && jumpHoldTimer > 0f)
                jumpHoldTimer -= Time.fixedDeltaTime;
            else
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
        float rayLength = capsule.height / 2f + 0.15f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundLayer);

        if (isGrounded)
        {
            lastOnGroundTime = jumpCoyoteTime;

            // se siamo a terra resetta subito canJump
            canJump = true;
            canBufferJump = true;

            if (!wasGrounded)
            {
                jumpHoldTimer = 0f;
                jumpBuffered = false;
            }

            canDash = true;
        }
        else
        {
            lastOnGroundTime -= Time.deltaTime;
        }
    }



    private void HandleWallDetection()
    {
        float rayLength = capsule.radius + 0.2f;
        Vector3 center = transform.position;

        bool hitRight = Physics.Raycast(center, Vector3.right, rayLength, groundLayer);
        bool hitLeft = Physics.Raycast(center, Vector3.left, rayLength, groundLayer);

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
    public void SetDead(bool value)
    {
        IsDead = value;
    }
    public void Bounce(float bounceForce)
    {
        // reset verticale per consistenza
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        // applica rimbalzo come un salto
        rb.AddForce(Vector3.up * bounceForce, ForceMode.VelocityChange);

        // reset stati come se fosse un salto
        canJump = false;
        lastOnGroundTime = 0f;
        isGrounded = false;
        jumpHoldTimer = 0f;
    }

}


