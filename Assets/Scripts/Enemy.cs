using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected int maxHealth = 50;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected int damage = 10;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected PlayerController player;
    protected bool isDead = false;

    protected enum AIState { Idle, Chase, Attack }
    protected AIState currentState = AIState.Idle;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        currentHealth = maxHealth;
        player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("Player not found!");
        }

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>().isTrigger = false;
        }

        gameObject.tag = "Enemy";

        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.color = Color.red;
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            sr.sortingOrder = 1;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        UpdateAI();
    }

    protected virtual void UpdateAI()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < 10f)
        {
            currentState = AIState.Chase;
        }
        else
        {
            currentState = AIState.Idle;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        HandleMovement();
    }

    protected virtual void HandleMovement()
    {
        if (currentState == AIState.Chase)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        LevelPortal[] portals = FindObjectsByType<LevelPortal>(FindObjectsSortMode.None);
        foreach (var portal in portals)
        {
            if (portal.requiresAllEnemiesDefeated)
            {
                portal.OnEnemyDefeated();
            }
        }

        Destroy(gameObject, 1f);
    }

    protected virtual void AttackPlayer()
    {
        player.TakeDamage(damage);
    }
}