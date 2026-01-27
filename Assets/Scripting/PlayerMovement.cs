using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Jump Tuning")]
    [SerializeField] private float jumpUpGravityMultiplier = 1.5f;


    [Header("Gravity Tuning (Hollow Knight style)")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float jumpCutGravityMultiplier = 2f;
    [SerializeField] private float jumpHangGravityMultiplier = 0.5f;
    [SerializeField] private float jumpHangVelocityThreshold = 0.1f;
    [SerializeField] private float maxFallSpeed = 20f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler input;

    [SerializeField] private bool isGrounded;

    // Jump buffer
    private bool jumpBuffered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        input = GetComponent<PlayerInputHandler>();

        rb.freezeRotation = true;
    }

    private void Update()
    {
        // buffer del salto
        if (input.JumpPressed)
            jumpBuffered = true;
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyHollowKnightGravity();
    }

    private void HandleMovement()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = input.MoveInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    private void HandleJump()
    {
        if (!jumpBuffered)
            return;

        if (!isGrounded)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;

        jumpBuffered = false;
    }

    private void ApplyHollowKnightGravity()
    {
        Vector3 velocity = rb.linearVelocity;

        // SALITA
        if (velocity.y > 0f)
        {
            // Gravità più forte in salita → stesso apex, salita più rapida
            velocity.y += Physics.gravity.y * (jumpUpGravityMultiplier - 1f) * Time.fixedDeltaTime;

            // Jump hang vicino all'apice
            if (Mathf.Abs(velocity.y) < jumpHangVelocityThreshold)
            {
                velocity.y += Physics.gravity.y * (jumpHangGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }

            // Jump cut
            if (!input.JumpHeld)
            {
                velocity.y += Physics.gravity.y * (jumpCutGravityMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }
        // DISCESA
        else if (velocity.y < 0f)
        {
            velocity.y += Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        }

        rb.linearVelocity = velocity;
    }


    private void CheckGround()
    {
        float rayLength = (capsule.height / 2f) + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            rayLength,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null) return;

        float rayLength = (capsule.height / 2f) + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * rayLength);
    }
}
