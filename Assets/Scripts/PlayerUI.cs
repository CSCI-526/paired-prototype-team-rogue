using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;
    
    private PlayerController _player;
    
    void Start()
    {
        // Get player controller - Using Unity 6's new API
        _player = FindFirstObjectByType<PlayerController>();
        
        if (_player == null)
        {
            Debug.LogError("PlayerController not found!");
            return;
        }
        
        // Initialize UI
        UpdateUI();
    }
    
    void Update()
    {
        if (_player != null)
        {
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        // Update HP bar
        if (healthBar != null)
        {
            healthBar.maxValue = _player.GetMaxHealth();
            healthBar.value = _player.GetCurrentHealth();
        }
        
        // Update Energy Bar
        if (energyBar != null)
        {
            energyBar.maxValue = _player.GetMaxEnergy();
            energyBar.value = _player.GetCurrentEnergy();
        }
    }
}