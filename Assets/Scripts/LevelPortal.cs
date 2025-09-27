using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] public PortalType portalType = PortalType.NextLevel;
    [SerializeField] private string targetLevelName = "";
    [SerializeField] public bool requiresAllEnemiesDefeated = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.cyan;
    
    public enum PortalType
    {
        NextLevel,
        SpecificLevel,
        HomeLevel
    }
    
    private SpriteRenderer _renderer;
    private bool _isActive = true;
    private int _enemyCount = 0;
    
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null)
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        if (requiresAllEnemiesDefeated)
        {
            _isActive = false;
            CountEnemiesInLevel();
            UpdateVisualState();
        }
        else
        {
            SetPortalActive(true);
        }
    }
    
    void CountEnemiesInLevel()
    {
        _enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
    }
    
    public void OnEnemyDefeated()
    {
        _enemyCount--;
        if (_enemyCount <= 0 && requiresAllEnemiesDefeated)
        {
            SetPortalActive(true);
        }
    }
    
    void SetPortalActive(bool active)
    {
        _isActive = active;
        UpdateVisualState();
    }
    
    void UpdateVisualState()
    {
        if (_renderer != null)
        {
            _renderer.color = _isActive ? activeColor : inactiveColor;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive) return;
        
        if (other.CompareTag("Player"))
        {
            ActivatePortal();
        }
    }
    
    void ActivatePortal()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        switch (portalType)
        {
            case PortalType.NextLevel:
                GameManager.Instance.LoadNextLevel();
                break;
            case PortalType.SpecificLevel:
                if (!string.IsNullOrEmpty(targetLevelName))
                {
                    GameManager.Instance.LoadLevel(targetLevelName);
                }
                else
                {
                    Debug.LogWarning("Target level name is empty!");
                }
                break;
            case PortalType.HomeLevel:
                GameManager.Instance.LoadHomeLevel();
                break;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = _isActive ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
        
        Gizmos.color = Color.yellow;
        Vector3 from = transform.position + Vector3.down * 0.5f;
        Vector3 to = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(from, to);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up, portalType.ToString());
        #endif
    }
}