using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Text energyText;
    
    private PlayerController player;
    
    void Start()
    {
        // get player Controller
        player = FindObjectOfType<PlayerController>();
        
        if (player == null)
        {
            Debug.LogError("找不到PlayerController!");
            return;
        }
        
        // Initialize UI
        UpdateUI();
    }
    
    void Update()
    {
        if (player != null)
        {
            UpdateUI();
        }
    }
    
    void UpdateUI()
    {
        // Update HP bar
        if (healthBar != null)
        {
            healthBar.maxValue = player.GetMaxHealth();
            healthBar.value = player.GetCurrentHealth();
        }
        
        // Update Energy Bar
        if (energyBar != null)
        {
            energyBar.maxValue = player.GetMaxEnergy();
            energyBar.value = player.GetCurrentEnergy();
        }
        
        // Update Text Display
        if (healthText != null)
        {
            healthText.text = $"HP: {player.GetCurrentHealth()}/{player.GetMaxHealth()}";
        }
        
        if (energyText != null)
        {
            energyText.text = $"Energy: {player.GetCurrentEnergy()}/{player.GetMaxEnergy()}";
        }
    }
}
