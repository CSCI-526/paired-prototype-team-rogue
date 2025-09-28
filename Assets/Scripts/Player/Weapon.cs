// Weapon.cs
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    private PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        
        var col2d = GetComponent<Collider2D>();
        if (col2d == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.0f, 0.25f);
            box.offset = new Vector2(0.6f, 0f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null || !player.IsAttacking()) return;
        
        if (!other.CompareTag("Enemy") && !other.transform.root.CompareTag("Enemy")) return;
        
        other.transform.root.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        player.OnHitEnemy();
    }

    public void AddAttackPower(int flat)
    {
        damage = Mathf.Max(1, damage + flat);
    }
}