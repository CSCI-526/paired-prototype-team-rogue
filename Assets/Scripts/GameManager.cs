using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Player Data")]
    private int _playerHealth = 100;
    private int _playerMaxHealth = 100;
    private int _playerEnergy = 0;
    private int _playerMaxEnergy = 100;
    
    [Header("Level Settings")]
    [SerializeField] private string homeSceneName = "HomeLevel";
    [SerializeField] private float sceneTransitionDelay = 0.5f;
    
    // Current level tracking
    private string _currentLevelName;
    private int _currentLevelIndex = 0;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Get current scene name
        _currentLevelName = SceneManager.GetActiveScene().name;
    }
    
    // Save player state before level transition
    public void SavePlayerState(PlayerController player)
    {
        if (player != null)
        {
            _playerHealth = player.GetCurrentHealth();
            _playerMaxHealth = player.GetMaxHealth();
            _playerEnergy = player.GetCurrentEnergy();
            _playerMaxEnergy = player.GetMaxEnergy();
        }
    }
    
    // Restore player state after level transition
    public void RestorePlayerState(PlayerController player)
    {
        if (player != null)
        {
            // Player will initialize with saved values
            player.SetHealth(_playerHealth, _playerMaxHealth);
            player.SetEnergy(_playerEnergy, _playerMaxEnergy);
        }
    }
    
    // Load a specific level
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelCoroutine(levelName));
    }
    
    // Load next level in sequence
    public void LoadNextLevel()
    {
        _currentLevelIndex++;
        string nextLevel = "Level_" + _currentLevelIndex;
        LoadLevel(nextLevel);
    }
    
    // Return to home
    public void LoadHomeLevel()
    {
        _currentLevelIndex = 0;
        LoadLevel(homeSceneName);
    }
    
    // Called when player dies
    public void OnPlayerDeath()
    {
        // Reset player stats
        _playerHealth = _playerMaxHealth;
        _playerEnergy = 0;
        
        // Return to home after delay
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // Wait for death animation or effects
        yield return new WaitForSeconds(2f);
        
        // Load home level
        LoadHomeLevel();
    }
    
    IEnumerator LoadLevelCoroutine(string levelName)
    {
        // Save current player state
        PlayerController currentPlayer = FindFirstObjectByType<PlayerController>();
        SavePlayerState(currentPlayer);
        
        // Optional: Add fade out effect here
        
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        // Load new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        _currentLevelName = levelName;
        
        // Find player in new scene and restore state
        yield return null; // Wait one frame for scene to initialize
        PlayerController newPlayer = FindFirstObjectByType<PlayerController>();
        RestorePlayerState(newPlayer);
        
        // Optional: Add fade in effect here
    }
    
    // Get current level name
    public string GetCurrentLevelName() => _currentLevelName;
    
    // Check if in home level
    public bool IsInHomeLevel() => _currentLevelName == homeSceneName;
}