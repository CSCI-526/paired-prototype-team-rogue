using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float attackCooldown = 2f;

    private float lastAttackTime = -999f;

    protected override void UpdateAI()
    {
        base.UpdateAI();

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= attackRange && distance > minDistance && Time.time > lastAttackTime + attackCooldown)
        {
            currentState = AIState.Attack;
        }
        else if (distance <= minDistance)
        {
            currentState = AIState.Chase;
        }
    }

    protected override void HandleMovement()
    {
        if (currentState == AIState.Attack)
        {
            rb.linearVelocity = Vector2.zero;
            PerformAttack();
        }
        else if (currentState == AIState.Chase)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            Vector2 direction = (player.transform.position - transform.position).normalized;
            if (distance < minDistance)
            {
                direction = -direction;
            }
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            base.HandleMovement();
        }
    }

    private void PerformAttack()
    {
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Vector2 dir = (player.transform.position - transform.position).normalized;
            proj.GetComponent<Projectile>().SetDirection(dir);
        }
        lastAttackTime = Time.time;
    }
}