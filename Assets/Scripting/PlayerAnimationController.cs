using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (movement == null) return;

        animator.SetFloat("Speed", Mathf.Abs(movement.Velocity.x));
        animator.SetFloat("VerticalSpeed", movement.Velocity.y);

        animator.SetBool("Grounded", movement.IsGrounded);
        animator.SetBool("Dashing", movement.IsDashing);
        animator.SetBool("WallSliding", movement.IsWallSliding);
        animator.SetBool("Dead", movement.IsDead);
        HandleFlip();
    }
    private void HandleFlip()
    {
        float xVelocity = movement.Velocity.x;

        if (xVelocity > 0.1f)
            spriteRenderer.flipX = false;
        else if (xVelocity < -0.1f)
            spriteRenderer.flipX = true;
    }
    public void PlayFootstep()
{
    AudioManager.Instance.PlayFootstep();
}


}
