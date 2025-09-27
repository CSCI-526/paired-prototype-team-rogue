using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float maxHealth = 100f;

    private float _currentHealth;
    private float _currentEnergy;
    private int _currentLevel = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _currentHealth = maxHealth;
        _currentEnergy = 0f;
    }

    public void SavePlayerState()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            _currentHealth = player.GetHealth();
            _currentEnergy = player.GetEnergy();
        }
    }

    public void RestorePlayerState()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetHealth(_currentHealth);
            player.SetEnergy(_currentEnergy);
        }
    }

    public void LoadNextLevel()
    {
        SavePlayerState();
        _currentLevel++;
        SceneManager.LoadScene("Level_" + _currentLevel);
        RestorePlayerState();
    }

    public void LoadLevel(string levelName)
    {
        SavePlayerState();
        SceneManager.LoadScene(levelName);
        RestorePlayerState();
    }

    public void LoadHomeLevel()
    {
        SavePlayerState();
        _currentLevel = 0;
        SceneManager.LoadScene("Home");
        RestorePlayerState();
    }

    public void OnPlayerDeath()
    {
        _currentHealth = maxHealth;
        _currentEnergy = 0f;
        Invoke("LoadHomeLevel", 1f);
    }
}