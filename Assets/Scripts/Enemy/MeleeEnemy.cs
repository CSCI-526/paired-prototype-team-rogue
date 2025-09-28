using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float baseAttackCooldown = 1f; // Base cooldown for regular swings
    [SerializeField] private float aggressiveAttackCooldown = 0.5f; // Faster cooldown when near
    [SerializeField] private float baseSwingAngle = 90f; // Base swing angle
    [SerializeField] private float aggressiveSwingAngle = 120f; // Wider swing when near
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform weapon;
    [SerializeField] private float rotationSpeed = 200f; // Degrees per second for spinning to face player

    private float lastAttackTime = -999f;
    private bool isSwinging = false;

    protected void Start()
    {
        base.Start();
        if (weaponPivot == null)
        {
            Debug.LogError("WeaponPivot not assigned in MeleeEnemy!");
        }
        if (weapon == null)
        {
            Debug.LogError("Weapon not assigned in MeleeEnemy!");
        }
        StartCoroutine(PeriodicSwing()); 
    }

    protected override void UpdateAI()
    {
        base.UpdateAI();

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= attackRange && Time.time > lastAttackTime + (distance <= attackRange ? aggressiveAttackCooldown : baseAttackCooldown))
        {
            currentState = AIState.Attack;
            PerformAttack(); 
        }
        else if (distance > attackRange)
        {
            currentState = AIState.Chase; 
        }
    }

    protected override void HandleMovement()
    {
        if (currentState == AIState.Attack)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            base.HandleMovement();
        }

        FacePlayer();
    }

    private void FacePlayer()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Adjust for sprite facing (e.g., down as default)
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void PerformAttack()
    {
        AttackPlayer();
        lastAttackTime = Time.time;
        if (weaponPivot != null && !isSwinging)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            float swingAngle = distance <= attackRange ? aggressiveSwingAngle : baseSwingAngle; // Pass correct angle
            StartCoroutine(SwingWeapon(swingAngle));
        }
    }

    System.Collections.IEnumerator PeriodicSwing()
    {
        while (true)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            float currentCooldown = distance <= attackRange ? aggressiveAttackCooldown : baseAttackCooldown;
            float currentSwingAngle = distance <= attackRange ? aggressiveSwingAngle : baseSwingAngle;

            if (Time.time > lastAttackTime + currentCooldown && !isSwinging)
            {
                isSwinging = true;
                yield return StartCoroutine(SwingWeapon(currentSwingAngle));
                isSwinging = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    System.Collections.IEnumerator SwingWeapon(float swingAngle)
    {
        float elapsed = 0f;
        Quaternion startRotation = weaponPivot.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, swingAngle); // Swing based on proximity

        while (elapsed < (Time.time > lastAttackTime + baseAttackCooldown ? baseAttackCooldown / 2 : aggressiveAttackCooldown / 2))
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (Time.time > lastAttackTime + baseAttackCooldown ? baseAttackCooldown / 2 : aggressiveAttackCooldown / 2);
            weaponPivot.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < (Time.time > lastAttackTime + baseAttackCooldown ? baseAttackCooldown / 2 : aggressiveAttackCooldown / 2))
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (Time.time > lastAttackTime + baseAttackCooldown ? baseAttackCooldown / 2 : aggressiveAttackCooldown / 2);
            weaponPivot.rotation = Quaternion.Lerp(targetRotation, startRotation, t);
            yield return null;
        }

        weaponPivot.rotation = startRotation;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time > lastAttackTime + (Vector2.Distance(transform.position, player.transform.position) <= attackRange ? aggressiveAttackCooldown : baseAttackCooldown))
        {
            currentState = AIState.Attack;
            PerformAttack();
        }
    }
}