using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack Settings")]
    [SerializeField] private Vector2 attackSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private float attackCooldown = 0.25f;

    [Header("Bounce")]
    [SerializeField] private float bounceForce = 9f;

    private float lastAttackTime;

    private Animator attackAnimator;
    private SpriteRenderer attackSpriteRenderer;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<PlayerInputHandler>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (sideAttackTransform != null)
        {
            attackAnimator = sideAttackTransform.GetComponent<Animator>();
            attackSpriteRenderer = sideAttackTransform.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (input.AttackPressed && CanAttack())
        {
            PerformAttack();
        }
    }

    private bool CanAttack()
    {
        if (playerMovement != null && playerMovement.isGrounded)
            return false;

        return Time.time >= lastAttackTime + attackCooldown;
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        if (attackSpriteRenderer != null)
            attackSpriteRenderer.flipX = sideAttackTransform.localPosition.x < 0f;

        if (attackAnimator != null)
            attackAnimator.SetTrigger("Attack");
            AudioManager.Instance.PlayAttack();


        Collider[] hits = Physics.OverlapBox(
            sideAttackTransform.position,
            attackSize * 0.5f,
            sideAttackTransform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Target"))
                continue;

            // RIMBALZA SOLO IL PLAYER
            playerMovement.Bounce(bounceForce);


            // una sola attivazione per attacco
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (sideAttackTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            sideAttackTransform.position,
            sideAttackTransform.rotation,
            Vector3.one
        );
        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(attackSize.x, attackSize.y, attackSize.y)
        );
    }
}
