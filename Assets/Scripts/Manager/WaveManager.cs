using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float waveDuration = 30f;
    [SerializeField] private int killRequirement = 30;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadiusMin = 8f;
    [SerializeField] private float spawnRadiusMax = 12f;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject basicEnemyPrefab;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.1f;
    [SerializeField] private int extraEnemiesPerWave = 5;
    [SerializeField] private float enemyHealthMultiplier = 1.2f;
    
    [Header("Spawn Area ")]
    [SerializeField] private BoxCollider2D spawnArea;   
    [SerializeField] private float spawnPadding = 0.5f; 
    
    [Header("Progression")]
    [SerializeField] private int maxWaves = 5;
    
    // Current wave stats
    private int _currentWave = 1;
    private float _waveTimer = 0f;
    private int _killCount = 0;
    private int _totalKillsNeeded;
    private bool _waveActive = false;
    private float _currentSpawnInterval;
    private float _spawnTimer = 0f;
    
    // References
    private PlayerController _player;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    
    // Events
    public delegate void WaveEvent(int waveNumber);
    public static event WaveEvent OnWaveStart;
    public static event WaveEvent OnWaveComplete;
    
    public delegate void KillEvent(int current, int required);
    public static event KillEvent OnKillCountUpdate;
    
    public delegate void TimerEvent(float timeRemaining);
    public static event TimerEvent OnWaveTimerUpdate;
    
    //fail states
    public enum FailReason { PlayerDead, TimeUp }

    public delegate void WaveFailedEvent(FailReason reason, int waveNumber);
    public static event WaveFailedEvent OnWaveFailed;
    private bool _hasEnded = false;
    
    void Start()
    {
        _player = FindFirstObjectByType<PlayerController>();
        if (_player == null)
        {
            Debug.LogError("Player not found in scene!");
        }
        
        // Start first wave after delay
        StartCoroutine(StartWaveAfterDelay(2f));
    }
    
    void Update()
    {
        if (!_waveActive || _hasEnded) return;


        if (_player != null && _player.GetHealth() <= 0f)
        {
            FailWave(FailReason.PlayerDead);
            return;
        }


        _waveTimer -= Time.deltaTime;
        OnWaveTimerUpdate?.Invoke(_waveTimer);
        
        if (_killCount >= _totalKillsNeeded)
        {
            CompleteWave();
            return;
        }
        else if (_waveTimer <= 0f)
        {
            if (_killCount < _totalKillsNeeded)
            {
                FailWave(FailReason.TimeUp);
                return;
            }
        }
        
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = _currentSpawnInterval;
        }
    }
    
    IEnumerator StartWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartWave();
    }
    
    public void StartWave()
    {
        _waveActive = true;
        _hasEnded = false;
        _waveTimer = waveDuration;
        _killCount = 0;
        
        // Calculate difficulty for this wave
        _totalKillsNeeded = killRequirement + ((_currentWave - 1) * extraEnemiesPerWave);
        _currentSpawnInterval = Mathf.Max(0.3f, spawnInterval - ((_currentWave - 1) * spawnIntervalDecreasePerWave));
        _spawnTimer = 0f;
        
        // Clear any remaining enemies
        ClearAllEnemies();
        
        // Notify UI
        OnWaveStart?.Invoke(_currentWave);
        OnKillCountUpdate?.Invoke(0, _totalKillsNeeded);
        
        Debug.Log($"Wave {_currentWave} started! Kill {_totalKillsNeeded} enemies in {waveDuration} seconds!");
    }
    
    void SpawnEnemy()
    {
        if (basicEnemyPrefab == null)
        {
            // Create temporary enemy if no prefab
            GameObject tempEnemy = new GameObject("Enemy");
            tempEnemy.AddComponent<Enemy>();
            
            // Position around player
            Vector2 spawnPosition = GetRandomSpawnPosition();
            tempEnemy.transform.position = spawnPosition;
            
            _activeEnemies.Add(tempEnemy);
        }
        else
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(basicEnemyPrefab, spawnPosition, Quaternion.identity);
            
            // Scale enemy health based on wave
            Enemy enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp != null && _currentWave > 1)
            {
                float healthMultiplier = Mathf.Pow(enemyHealthMultiplier, _currentWave - 1);
                // You'll need to add a method to Enemy to set max health
            }
            
            _activeEnemies.Add(enemy);
        }
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        if (spawnArea != null)
        {
            Bounds b = spawnArea.bounds;
            
            float padX = Mathf.Min(spawnPadding, b.size.x * 0.5f - 0.01f);
            float padY = Mathf.Min(spawnPadding, b.size.y * 0.5f - 0.01f);

            float x = Random.Range(b.min.x + padX, b.max.x - padX);
            float y = Random.Range(b.min.y + padY, b.max.y - padY);
            return new Vector2(x, y);
        }
        
        if (_player == null) return Vector2.zero;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        return (Vector2)_player.transform.position + offset;
    }
    
    public void OnEnemyKilled()
    {
        _killCount++;
        OnKillCountUpdate?.Invoke(_killCount, _totalKillsNeeded);
        
        Debug.Log($"Enemy killed! {_killCount}/{_totalKillsNeeded}");
    }
    
    void CompleteWave()
    {
        _waveActive = false;
        OnWaveComplete?.Invoke(_currentWave);

        ClearAllEnemies();

        // 到达最大关卡：结束
        if (_currentWave >= maxWaves)
        {
            Debug.Log($"All waves cleared at wave {_currentWave}. Game Over.");
            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.Show("Game Over");
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDeath(); // 兜底
            }
            return;
        }

        // 未到最大关卡：弹 Powerup，下一关
        if (PowerupManager.Instance != null)
        {
            PowerupManager.Instance.ShowPowerupSelection();
        }

        _currentWave++;
    }
    
    void FailWave(FailReason reason)
    {
        if (_hasEnded) return;   // 双重保护
        _hasEnded = true;
        _waveActive = false;

        Debug.Log($"Wave {_currentWave} failed! Reason: {reason}");

        // 通知 UI（可选）
        OnWaveFailed?.Invoke(reason, _currentWave);

        // 清理敌人（避免残留）
        ClearAllEnemies();

        // 游戏处理：
        // - 若是玩家死亡：PlayerController.Die() 已经会调用 GameManager.OnPlayerDeath()，这里就不重复调用。
        // - 若是时间耗尽：在这里调用 GameManager.OnPlayerDeath()。
        if (reason == FailReason.TimeUp && GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
    
    void ClearAllEnemies()
    {
        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        _activeEnemies.Clear();
    }
    
    public void StartNextWave()
    {
        StartCoroutine(StartWaveAfterDelay(1f));
    }
    
    // Getters for UI
    public int CurrentWave => _currentWave;
    public int KillCount => _killCount;
    public int KillsNeeded => _totalKillsNeeded;
    public float TimeRemaining => _waveTimer;
}