using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum PowerupType
{
    AttackPower,     // +flat damage
    AttackSpeed,     // -% attack cooldown
    MovementSpeed,   // +% move speed
    MaxHP,           // +max HP
    HPRegen,         // +HP regen per second
    EnergyRegen      // +Energy regen per second
}

[System.Serializable]
public class PowerupData
{
    public string powerupName;
    public string description;
    public PowerupType type;
    public float value;
    public Sprite icon;
    public Rarity rarity = Rarity.Common; 
    public float weightOverride = -1f;    
}

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject powerupSelectionPanel;
    [SerializeField] private TMP_Text headerText;                     // "Level Cleared!"
    [SerializeField] private Button[] powerupButtons = new Button[3]; // the 3 option buttons
    [SerializeField] private TMP_Text[] powerupTitles = new TMP_Text[3];
    [SerializeField] private TMP_Text[] powerupDescriptions = new TMP_Text[3];
    [SerializeField] private Button confirmButton;                    // "Continue"

    [Header("Pool")]
    [SerializeField] private PowerupPoolAsset poolAsset;
    [SerializeField] private List<PowerupData> powerupPool = new List<PowerupData>();

    private PowerupData[] _candidates = new PowerupData[3];
    private int _selectedIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (powerupSelectionPanel) powerupSelectionPanel.SetActive(false);
        if (confirmButton)
        {
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.interactable = false; // locked until an option is chosen
        }

        for (int i = 0; i < powerupButtons.Length; i++)
        {
            int idx = i; // capture
            if (powerupButtons[i]) powerupButtons[i].onClick.AddListener(() => SelectPowerup(idx));
        }
    }

    public void ShowPowerupSelection()
    {
        if (headerText) headerText.text = "Level Cleared!";
        if (powerupSelectionPanel)
        {
            powerupSelectionPanel.transform.SetAsLastSibling();
            powerupSelectionPanel.SetActive(true);
        }
        Time.timeScale = 0f;

        _selectedIndex = -1;
        if (confirmButton) confirmButton.interactable = false;

        PowerupData[] picks = null;

        if (poolAsset != null)
        {
            picks = poolAsset.Draw(3); 
        }
        else if (powerupPool != null && powerupPool.Count > 0)
        {
            picks = new PowerupData[3];
            for (int i = 0; i < 3; i++)
                picks[i] = powerupPool[Random.Range(0, powerupPool.Count)];
        }
        else
        {
            for (int i = 0; i < powerupButtons.Length; i++)
            {
                if (powerupButtons[i]) powerupButtons[i].interactable = false;
                if (powerupTitles.Length > i && powerupTitles[i]) powerupTitles[i].text = "No Upgrade";
                if (powerupDescriptions.Length > i && powerupDescriptions[i]) powerupDescriptions[i].text = "No upgrades available.";
                _candidates[i] = null;
            }
            if (confirmButton) confirmButton.interactable = true;
            return;
        }
        
        for (int i = 0; i < 3; i++)
        {
            _candidates[i] = picks[i];
            var d = picks[i];
            if (d == null) continue;

            if (powerupTitles.Length > i && powerupTitles[i]) powerupTitles[i].text = d.powerupName;
            if (powerupDescriptions.Length > i && powerupDescriptions[i]) powerupDescriptions[i].text = d.description;
            if (powerupButtons.Length > i && powerupButtons[i]) powerupButtons[i].interactable = true;
        }
    }

    private void SelectPowerup(int index)
    {
        _selectedIndex = index;

        // simple feedback: disable the chosen button
        for (int i = 0; i < powerupButtons.Length; i++)
            if (powerupButtons[i]) powerupButtons[i].interactable = (i != _selectedIndex);

        if (confirmButton) confirmButton.interactable = true;
    }

    private void ConfirmSelection()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _candidates.Length && _candidates[_selectedIndex] != null)
            ApplyPowerup(_candidates[_selectedIndex]);

        // reset UI
        for (int i = 0; i < powerupButtons.Length; i++)
            if (powerupButtons[i]) powerupButtons[i].interactable = true;

        if (powerupSelectionPanel) powerupSelectionPanel.SetActive(false);
        Time.timeScale = 1f;

        ContinueNextWave();
    }

    private void ContinueWithoutSelection()
    {
        if (powerupSelectionPanel) powerupSelectionPanel.SetActive(false);
        Time.timeScale = 1f;
        ContinueNextWave();
    }

    private void ContinueNextWave()
    {
        var wave = FindFirstObjectByType<WaveManager>();
        if (wave) wave.StartNextWave();
    }

    private void ApplyPowerup(PowerupData p)
    {
        var player = FindFirstObjectByType<PlayerController>();
        var weapon = FindFirstObjectByType<Weapon>();
        if (p == null || player == null) return;

        switch (p.type)
        {
            case PowerupType.AttackPower:
                if (weapon) weapon.AddAttackPower(Mathf.RoundToInt(p.value));
                break;
            case PowerupType.AttackSpeed:
                player.AddAttackSpeedPercent(p.value);   // 0.2 => -20% CD
                break;
            case PowerupType.MovementSpeed:
                player.AddMoveSpeedPercent(p.value);     // 0.15 => +15% move
                break;
            case PowerupType.MaxHP:
                player.AddMaxHealth(p.value);
                break;
            case PowerupType.HPRegen:
                player.AddHealthRegen(p.value);
                break;
            case PowerupType.EnergyRegen:
                player.AddEnergyRegen(p.value);
                break;
        }
    }
}
