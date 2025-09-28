// Enemy.cs
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy_New : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int   maxHP = 30;
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Contact Damage")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactInterval = 0.5f;
    [SerializeField] private string playerTag = "Player"; 

    [Header("Colliders (assign manually)")]
    [SerializeField] private Collider2D bodyCollider;    // 
    [SerializeField] private Collider2D combatTrigger;   // 

    private int hp;
    private Rigidbody2D rb;
    private Transform player;
    private float nextContactTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        if (bodyCollider && bodyCollider.isTrigger)
            Debug.LogWarning($"{name}: bodyCollider should NOT be trigger.");
        if (combatTrigger && !combatTrigger.isTrigger)
            Debug.LogWarning($"{name}: combatTrigger SHOULD be trigger.");

        if (string.IsNullOrEmpty(tag) || tag == "Untagged") tag = "Enemy";
    }

    private void OnEnable()
    {
        hp = Mathf.Max(1, maxHP);
        var pc = FindFirstObjectByType<PlayerController>();
        player = pc ? pc.transform : null;
    }

    private void FixedUpdate()
    {
        if (!player) return;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextContactTime) return;
        if (!other || !other.CompareTag(playerTag)) return;  

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>(); 
        if (pc == null) return;

        pc.TakeDamage(contactDamage);
        nextContactTime = Time.time + contactInterval;
    }
    
    public void TakeDamage(int dmg)
    {
        hp -= Mathf.Max(0, dmg);
        if (hp <= 0) Die();
    }

    private void Die()
    {
        var wave = FindFirstObjectByType<WaveManager>();
        if (wave) wave.OnEnemyKilled();
        Destroy(gameObject);
    }
    
    public void MultiplyMaxHP(float mul)
    {
        maxHP = Mathf.Max(1, Mathf.CeilToInt(maxHP * Mathf.Max(0.01f, mul)));
        hp = maxHP;
    }
}
