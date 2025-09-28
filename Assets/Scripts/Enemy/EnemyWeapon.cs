using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    private MeleeEnemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<MeleeEnemy>();
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 0.2f);
        }

        // Add a simple white rectangle sprite if none exists
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            Texture2D whiteTex = new Texture2D(1, 1);
            whiteTex.SetPixel(0, 0, Color.white);
            whiteTex.Apply();
            sr.sprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            sr.transform.localScale = new Vector3(1f, 0.2f, 1f); // Scale to rectangle shape
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("No PlayerController found on collided object: " + other.name);
            }
        }
        else
        {
            Debug.LogWarning("Collided object " + other.name + " does not have Player tag");
        }
    }
}