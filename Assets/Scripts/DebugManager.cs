using UnityEngine;

public class DebugManager : MonoBehaviour
{
    
    private WaveManager wave;
    private PlayerController player;

    void Awake()
    {
        wave = FindFirstObjectByType<WaveManager>();
        player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (wave != null && Input.GetKeyDown(KeyCode.Alpha5))
            wave.OnEnemyKilled();

        if (player != null && Input.GetKeyDown(KeyCode.Alpha4))
            player.TakeDamage(20f);
    }
}
