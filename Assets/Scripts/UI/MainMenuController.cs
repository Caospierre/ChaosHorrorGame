using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "DevScene";

    private void Start()
    {
        // Setup button listeners
        if (startButton) startButton.onClick.AddListener(OnStartGame);
        if (creditsButton) creditsButton.onClick.AddListener(OnCredits);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);
        if (backButton) backButton.onClick.AddListener(OnBack);

        // Show main panel by default
        ShowMainPanel();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (startButton) startButton.onClick.RemoveListener(OnStartGame);
        if (creditsButton) creditsButton.onClick.RemoveListener(OnCredits);
        if (quitButton) quitButton.onClick.RemoveListener(OnQuit);
        if (backButton) backButton.onClick.RemoveListener(OnBack);
    }

    private void OnStartGame()
    {
        HotelLayoutManager.Instance.RestartWithNewSeed();
        SceneLoader.Instance.LoadScene(gameSceneName);
    }

    private void OnCredits()
    {
        ShowCreditsPanel();
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnBack()
    {
        ShowMainPanel();
    }

    private void ShowMainPanel()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (creditsPanel) creditsPanel.SetActive(false);
    }

    private void ShowCreditsPanel()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(true);
    }
}

