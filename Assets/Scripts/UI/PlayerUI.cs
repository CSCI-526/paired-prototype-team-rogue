using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider energySlider;

    private PlayerController player;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
        else
        {
            Debug.Log("PlayerUI Start - Player found");
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = player.MaxHealth;
            healthSlider.minValue = 0;
            healthSlider.value = player.GetHealth();
        }
        else
        {
            Debug.LogError("Health Slider not assigned in PlayerUI!");
        }

        if (energySlider != null)
        {
            energySlider.maxValue = player.MaxEnergy;
            energySlider.minValue = 0;
            energySlider.value = player.GetEnergy();
        }
        else
        {
            Debug.LogError("Energy Slider not assigned in PlayerUI!");
        }
    }

    void Update()
    {
        if (player != null)
        {
            if (healthSlider != null) {
                healthSlider.value = player.GetHealth();
                Debug.Log("Health Slider updated to: " + healthSlider.value);
            }
            if (energySlider != null) energySlider.value = player.GetEnergy();
        }
    }
}