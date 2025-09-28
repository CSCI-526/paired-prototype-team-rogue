using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private Slider killProgressBar;
    
    [Header("Wave Complete Panel")]
    [SerializeField] private GameObject waveCompletePanel;
    [SerializeField] private Text waveCompleteTitleText;
    
    void OnEnable()
    {
        // Subscribe to events
        WaveManager.OnWaveStart += UpdateWaveNumber;
        WaveManager.OnWaveComplete += ShowWaveComplete;
        WaveManager.OnKillCountUpdate += UpdateKillCount;
        WaveManager.OnWaveTimerUpdate += UpdateTimer;
    }
    
    void OnDisable()
    {
        // Unsubscribe from events
        WaveManager.OnWaveStart -= UpdateWaveNumber;
        WaveManager.OnWaveComplete -= ShowWaveComplete;
        WaveManager.OnKillCountUpdate -= UpdateKillCount;
        WaveManager.OnWaveTimerUpdate -= UpdateTimer;
    }
    
    void Start()
    {
        // Hide wave complete panel at start
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
    }
    
    void UpdateWaveNumber(int waveNumber)
    {
        if (waveNumberText != null)
            waveNumberText.text = $"Level: {waveNumber}";
            
        // Hide wave complete panel when new wave starts
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
    }
    
    void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {timeRemaining:F1}s"; 
            if (timeRemaining <= 5f)      timerText.color = Color.red;
            else if (timeRemaining <= 10) timerText.color = Color.yellow;
            else                          timerText.color = Color.white;
        }
    }
    
    void UpdateKillCount(int current, int required)
    {
        if (killCountText != null)
            killCountText.text = $"Kills: {current}/{required}";

        if (killProgressBar != null)
        {
            killProgressBar.maxValue = required;
            killProgressBar.value    = current;
        }
    }
    
    void ShowWaveComplete(int waveNumber)
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
            
            if (waveCompleteTitleText != null)
                waveCompleteTitleText.text = $"Wave {waveNumber} Complete!";
        }
    }
}