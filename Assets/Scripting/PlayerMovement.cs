using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private PlayerInputHandler input;

    [SerializeField] private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInputHandler>();

        if (rb == null)
            Debug.LogError("[PlayerMovement] Rigidbody mancante.", this);

        if (input == null)
            Debug.LogError("[PlayerMovement] PlayerInputHandler mancante.", this);
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
        if (!input.JumpPressed)
            return;

        if (!isGrounded)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    // utile per debug visivo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 0.05f,
            transform.position + Vector3.up * 0.05f + Vector3.down * groundCheckDistance
        );
    }
}
