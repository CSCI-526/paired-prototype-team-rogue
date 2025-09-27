using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 0.2f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player.IsAttacking() && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                player.OnHitEnemy();
            }
        }
    }
}