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

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInputHandler input;

    [SerializeField] private bool isGrounded;

    // Input buffer
    private bool jumpBuffered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        input = GetComponent<PlayerInputHandler>();

        if (rb == null)
            Debug.LogError("[PlayerMovement] Rigidbody mancante.", this);

        if (capsule == null)
            Debug.LogError("[PlayerMovement] CapsuleCollider mancante.", this);

        if (input == null)
            Debug.LogError("[PlayerMovement] PlayerInputHandler mancante.", this);
    }

    private void Update()
    {
        // Cattura input in Update
        if (input.JumpPressed)
            jumpBuffered = true;
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
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

        Debug.Log("salto");
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
