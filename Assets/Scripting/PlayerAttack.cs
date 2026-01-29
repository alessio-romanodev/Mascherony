using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private PlayerInputHandler input;

    [Header("Attack Settings")]
    [SerializeField] private Vector2 attackSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private float attackCooldown = 0.25f;

    private float lastAttackTime;

    [SerializeField] private Animator attackAnimator;
    [SerializeField] private SpriteRenderer attackSpriteRenderer;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<PlayerInputHandler>();

        if (sideAttackTransform != null)
        {
            attackAnimator = sideAttackTransform.GetComponent<Animator>();
            attackSpriteRenderer = sideAttackTransform.GetComponent<SpriteRenderer>();
        }

        if (attackAnimator == null)
            Debug.LogWarning("PlayerAttack: Animator non trovato su SideAttackTransform");

        if (attackSpriteRenderer == null)
            Debug.LogWarning("PlayerAttack: SpriteRenderer non trovato su SideAttackTransform");
    }

    private void Update()
    {
        if (input.AttackPressed && CanAttack())
        {
            PerformAttack();
        }
        if (sideAttackTransform.localPosition.x < 0f)
            attackSpriteRenderer.flipX = true;
        else attackSpriteRenderer.flipX = false;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // Trigger animazione
        if (attackAnimator != null)
        {
            attackAnimator.SetTrigger("Attack");
        }

        Collider[] hits = Physics.OverlapBox(
            sideAttackTransform.position,
            attackSize * 0.5f,
            sideAttackTransform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Target"))
            {
                Debug.Log("Colpito target: " + hit.name);
                Destroy(hit.gameObject);
                // altra logica
            }
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
