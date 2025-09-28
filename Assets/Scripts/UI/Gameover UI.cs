// GameOverUI.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    // One instance per scene.
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (panel) panel.SetActive(false);
        if (restartButton) restartButton.onClick.AddListener(Restart);
    }

    public void Show(string title = "Game Over")
    {
        if (titleText) titleText.text = title;
        if (panel)
        {
            panel.transform.SetAsLastSibling(); // ensure on top
            panel.SetActive(true);
        }
        Time.timeScale = 0f; // pause
    }

    private void Restart()
    {
        Time.timeScale = 1f; // resume
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}