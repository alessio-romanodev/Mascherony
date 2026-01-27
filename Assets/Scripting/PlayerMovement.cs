using UnityEngine;
using System.Collections; // necessario per IEnumerator


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

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 6f;
    [SerializeField] private float wallJumpForceY = 8f;
    [SerializeField] private float wallJumpCoyoteTime = 0.2f; // memoria del muro
    private int wallDir = 0; // -1 = muro a sinistra, 1 = muro a destra, 0 = nessun muro
    private float lastOnWallTime = 0f;
    private bool isWallJumping = false;
    [SerializeField] private float wallJumpDuration = 0.2f; // durata spinta orizzontale
    private float wallJumpTimer = 0f;


    [Header("Gravity Tuning (Hollow Knight style)")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float jumpCutGravityMultiplier = 2f;
    [SerializeField] private float jumpHangGravityMultiplier = 0.5f;
    [SerializeField] private float jumpHangVelocityThreshold = 0.1f;
    [SerializeField] private float maxFallSpeed = 20f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler input;
    private Collider currentPlatform;
    private Collider lastPlatformTouched;


    [SerializeField] private bool isGrounded;

    // Jump buffer e flag
    private bool jumpBuffered;
    private bool canJump = true;

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

        // Riduzione timer coyote wall
        lastOnWallTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleWallCollision();
        HandleMovement();
        HandleJump();
        ApplyHollowKnightGravity();
    }

    private void HandleMovement()
    {
        Vector3 velocity = rb.linearVelocity;

        // Se non stiamo wall jumpando o la durata è finita
        if (!isWallJumping)
        {
            if (wallDir == 0 || Mathf.Sign(input.MoveInput) != wallDir)
                velocity.x = input.MoveInput * moveSpeed;
        }
        else
        {
            // countdown timer wall jump
            wallJumpTimer -= Time.fixedDeltaTime;
            if (wallJumpTimer <= 0f)
                isWallJumping = false;
        }

        rb.linearVelocity = velocity;
    }


    private void HandleJump()
    {
        if (!jumpBuffered) return;

        // DROP THROUGH PLATFORM
        if (isGrounded && input.DropDown && input.JumpPressed)
        {
            DropThroughPlatform();
            jumpBuffered = false;
            return;
        }


        // WALL JUMP
        if (!isGrounded && wallDir != 0)
        {
            jumpBuffered = false;
            int jumpDir = -wallDir;
            Vector3 force = new Vector3(jumpDir * wallJumpForceX, wallJumpForceY, 0f);

            Vector3 velocity = rb.linearVelocity;
            if (velocity.y < 0f)
                force.y -= velocity.y;

            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.AddForce(force, ForceMode.Impulse);

            isWallJumping = true;
            wallJumpTimer = wallJumpDuration;
            wallDir = 0;
            return;
        }

        // SALTO NORMALE
        if (!canJump || !isGrounded) return;

        Vector3 normalVelocity = rb.linearVelocity;
        normalVelocity.y = jumpForce;
        rb.linearVelocity = normalVelocity;

        jumpBuffered = false;
        canJump = false;
    }




    private void ApplyHollowKnightGravity()
    {
        Vector3 velocity = rb.linearVelocity;

        // SALITA
        if (velocity.y > 0f)
        {
            velocity.y += Physics.gravity.y * (jumpUpGravityMultiplier - 1f) * Time.fixedDeltaTime;

            if (Mathf.Abs(velocity.y) < jumpHangVelocityThreshold)
                velocity.y += Physics.gravity.y * (jumpHangGravityMultiplier - 1f) * Time.fixedDeltaTime;

            if (!input.JumpHeld)
                velocity.y += Physics.gravity.y * (jumpCutGravityMultiplier - 1f) * Time.fixedDeltaTime;
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

        bool wasGrounded = isGrounded;

        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            canJump = true;
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



    private void HandleWallCollision()
    {
        float rayLength = capsule.radius + 0.2f;

        Vector3 originCenter = transform.position;
        Vector3 originTop = originCenter + Vector3.up * (capsule.height / 2f - capsule.radius);
        Vector3 originBottom = originCenter + Vector3.down * (capsule.height / 2f - capsule.radius);

        // Rileva muro a destra
        bool hitRight = Physics.Raycast(originCenter, Vector3.right, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore) ||
                        Physics.Raycast(originTop, Vector3.right, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore) ||
                        Physics.Raycast(originBottom, Vector3.right, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        // Rileva muro a sinistra
        bool hitLeft = Physics.Raycast(originCenter, Vector3.left, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore) ||
                       Physics.Raycast(originTop, Vector3.left, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore) ||
                       Physics.Raycast(originBottom, Vector3.left, rayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        // Aggiorna wallDir e memoria coyote muro
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

        // Blocca il movimento verso il muro
        Vector3 velocity = rb.linearVelocity;
        if ((wallDir == 1 && input.MoveInput > 0) || (wallDir == -1 && input.MoveInput < 0))
            velocity.x = 0f;

        rb.linearVelocity = velocity;

        // Debug visivo
        Debug.DrawRay(originCenter, Vector3.right * rayLength, hitRight ? Color.green : Color.red);
        Debug.DrawRay(originTop, Vector3.right * rayLength, hitRight ? Color.green : Color.red);
        Debug.DrawRay(originBottom, Vector3.right * rayLength, hitRight ? Color.green : Color.red);

        Debug.DrawRay(originCenter, Vector3.left * rayLength, hitLeft ? Color.green : Color.red);
        Debug.DrawRay(originTop, Vector3.left * rayLength, hitLeft ? Color.green : Color.red);
        Debug.DrawRay(originBottom, Vector3.left * rayLength, hitLeft ? Color.green : Color.red);
    }

    private void OnDrawGizmosSelected()
    {
        if (capsule == null) return;

        float rayLength = (capsule.height / 2f) + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;

        Gizmos.color = isGrounded ? Color.yellow : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * rayLength);
    }

    private void DropThroughPlatform()
    {
        if (currentPlatform == null) return;
        StartCoroutine(DropCoroutine(currentPlatform));
    }

    private IEnumerator DropCoroutine(Collider platform)
    {
        Physics.IgnoreCollision(platform, capsule, true); // ignora la piattaforma
        yield return new WaitForSeconds(0.3f); // tempo per passare attraverso
        if (platform != null)
            Physics.IgnoreCollision(platform, capsule, false); // ripristina collisione
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Platform") && rb.linearVelocity.y > 0)
        {
            //Physics.IgnoreCollision(collision.collider, capsule, true);
            collision.collider.isTrigger = true;
            lastPlatformTouched = collision.collider;
            StartCoroutine(ReEnableCollision(collision.collider));
        }
    }

    private IEnumerator ReEnableCollision(Collider platform)
    {
        yield return new WaitForSeconds(0.1f); // passa attraverso
        if (platform != null)
            lastPlatformTouched.isTrigger = true;
        //Physics.IgnoreCollision(platform, capsule, false);
    }

}
