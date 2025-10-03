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
        if (panel) panel.SetActive(false);

        Time.timeScale = 1f;
        
        var temp = new GameObject("~DDOL_Cleaner");
        DontDestroyOnLoad(temp);
        var ddolScene = temp.scene;
        var roots = ddolScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != temp) Destroy(roots[i]);
        }
        Destroy(temp);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}