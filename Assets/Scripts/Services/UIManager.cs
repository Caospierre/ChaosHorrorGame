using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public bool canPlayerMove = true; // To freeze the player

    [Header("Clue Display")]
    [SerializeField] private CanvasGroup clueDisplayRoot;
    [SerializeField] private Canvas finishScreenCanvas;

    [SerializeField] private TextMeshProUGUI clueText;

    [Space]
    [SerializeField] private InputController input;

    [Header("Candy Defeat Messages")]
    [SerializeField] private CandyDefeatMessagesSO candyDefeatMessages;

    [Space]

    [Header("Interaction Prompt")]
    [SerializeField] private TextMeshProUGUI promptText;
    [Space]

    [Header("Candy Display")]
    [SerializeField] private CanvasGroup candyDisplayCG;
    [SerializeField] private TextMeshProUGUI candyText;

    [Space]
    [Header("Escape menu")]
    [SerializeField] private CanvasGroup escapeMenuCG;

    [Space]
    [Header("Finish Screen")]
    [SerializeField] private CanvasGroup finishScreenRoot;
    [SerializeField] private TextMeshProUGUI finishTitle;
    [SerializeField] private TextMeshProUGUI finishMessage;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Image victoryImage;
    [SerializeField] private Image defeatImage;

    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelCredits;

    [Header("Monster TV Audio")]
    [SerializeField] private AudioClip tvAudioStartClip;
    [SerializeField] private AudioClip tvAudioLoopClip;

    private Coroutine TvCoroutine;
    private InteractableClueTelevision currentClue = null;
    private bool isShowingMonsterTv = false;
    private bool isShowingEscapeScreen = false;
    private bool isShowingKeypad = false;

    public event Action OnMonsterTvClosed;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (finishScreenRoot != null)
        {
            finishScreenRoot.alpha = 0;
            finishScreenRoot.blocksRaycasts = false;
            finishScreenRoot.interactable = false;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);

        if (HotelLayoutManager.Instance != null)
            UpdateCandyDisplay(HotelLayoutManager.Instance.CurrentCandy);

        if (panelMainMenu && panelCredits != null)
        {
            panelMainMenu.SetActive(true);
            panelCredits.SetActive(false);
        }    
        
        if (candyDisplayCG != null)
            ShowCandyDisplay();
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.ClosePressed += HandleClosePressed;
            input.EscapePressed += HandleEscapePressed;
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.ClosePressed -= HandleClosePressed;
            input.EscapePressed -= HandleEscapePressed;
        }
            

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestart);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenu);
    }

    public void OnRestart()
    {
        Debug.Log("Restart button clicked");
        if (SceneLoader.Instance != null)
            HotelLayoutManager.Instance.RestartWithNewSeed();
        else
            Debug.LogError("SceneLoader not found in scene!");

        if (candyText != null)
            candyText.gameObject.SetActive(true);
    }

    public void OnMainMenu()
    {
        Debug.Log("Main menu button clicked");
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("Main Menu Scene");
        else
            Debug.LogError("SceneLoader not found in scene!");
    }

    private void HandleClosePressed()
    {
        if (currentClue != null)
        {
            HideClueUI();
            if (input != null)
                input.EnableGameplayInputs();
            currentClue.isInteractable = true;
            currentClue = null;
            return;
        }

        if (isShowingMonsterTv)
        {
            OnMonsterTvClosed?.Invoke();
            HideClueUI();
            isShowingMonsterTv = false;
        }
    }

    private void HandleEscapePressed()
    {
        if (currentClue != null || isShowingMonsterTv)
            return;

        if (isShowingEscapeScreen)
            HideEscapeScreen();
        else
            ShowEscapeScreen();
    }

    public void DisplayClueUI(InteractableClueTelevision tv)
    {
        if (currentClue != null)
            return;

        if (input != null)
            input.EnableUIInputs();

        HideCandyDisplay();

        canPlayerMove = false;

        clueText.text = tv.clue.ToString();
        clueDisplayRoot.alpha = 1;
        clueDisplayRoot.blocksRaycasts = true;
        currentClue = tv;

        if (TvCoroutine != null)
            StopCoroutine(TvCoroutine);

        TvCoroutine = StartCoroutine(TvAudioRoutine());
    }

    private void HideClueUI()
    {
        clueDisplayRoot.alpha = 0;
        clueDisplayRoot.blocksRaycasts = false;
        clueText.text = "";

        if (candyText != null)
            candyText.gameObject.SetActive(true);

        canPlayerMove = true;
        ShowCandyDisplay();
    }

    public void UpdateCandyDisplay(int amount)
    {
        if (candyText != null)
            candyText.text = amount.ToString();
        Debug.Log(amount);
    }

    private void ShowCandyDisplay()
    {
        candyDisplayCG.alpha = 1;
    }

    private void HideCandyDisplay()
    {
        candyDisplayCG.alpha = 0;
    }

    public void ShowVictory()
    {
        defeatImage.gameObject.SetActive(false);
        victoryImage.gameObject.SetActive(true);
        ShowFinishScreen("VICTORY!", "You escaped the haunted hotel!");
    }

    public void ShowDefeat()
    {
        defeatImage.gameObject.SetActive(true);
        victoryImage.gameObject.SetActive(false);
        ShowFinishScreen("GAME OVER", "The hotel consumed you...");
    }

    public void ShowCandyDefeat()
    {
        defeatImage.gameObject.SetActive(true);
        victoryImage.gameObject.SetActive(false);

        string message = candyDefeatMessages != null
            ? candyDefeatMessages.GetRandomMessage()
            : "You ran out of candy!";

        ShowFinishScreen("GAME OVER", message);
    }

    private void ShowFinishScreen(string title, string message)
    {
        Debug.Log($"ShowFinishScreen called with title: {title}");

        if (finishScreenRoot == null || finishTitle == null || finishMessage == null)
        {
            Debug.LogWarning("Finish screen UI not set up - components are null");
            return;
        }

        HideCandyDisplay();
        canPlayerMove = false;
        if (input != null)
            input.EnableUIInputs();

        finishTitle.text = title;
        finishMessage.text = message;

        if (candyText != null)
            candyText.gameObject.SetActive(false);

        finishScreenRoot.alpha = 1;
        finishScreenRoot.blocksRaycasts = true;
        finishScreenRoot.interactable = true;

        Debug.Log("Finish screen shown successfully!");
    }

    public void DisplayMonsterTv(string message)
    {
        HideCandyDisplay();
        isShowingMonsterTv = true;

        if (input != null)
            input.EnableUIInputs();

        clueText.text = message;
        clueDisplayRoot.alpha = 1;

        if (TvCoroutine != null)
            StopCoroutine(TvCoroutine);

        TvCoroutine = StartCoroutine(TvAudioRoutine());
    }

    private IEnumerator TvAudioRoutine()
    {
        if (SoundManager.Instance == null)
            yield break;

        if (tvAudioStartClip == null || tvAudioLoopClip == null)
            yield break;

        // Play TV startup sound using the general SFX channel
        SoundManager.Instance.PlayOneShotNonLoop(tvAudioStartClip);

        // Wait for it to finish
        yield return new WaitWhile(() => SoundManager.Instance.IsSfxPlaying(tvAudioStartClip));

        // Begin looping TV sound
        SoundManager.Instance.PlayLoop(tvAudioLoopClip);

        // Wait until the TV display is closed
        while (clueDisplayRoot.alpha > 0)
            yield return null;

        // Stop the TV audio once the display ends
        SoundManager.Instance.StopLoop();
    }

    public void MonsterTvAttackFinished()
    {
        isShowingMonsterTv = false;

        if (input != null)
            input.EnableGameplayInputs();

        ShowCandyDisplay();
    }
    
    public void ShowInteractionPrompt(string prompt)
    {
        if (promptText == null)
            return;

        promptText.text = prompt;
    }

    public void ClearInteractionPrompt()
    {
        if (promptText == null)
            return;

        promptText.text = "";
    }

    private void ShowEscapeScreen()
    {
        if (escapeMenuCG == null || isShowingEscapeScreen || isShowingKeypad)
            return;

        isShowingEscapeScreen = true;
        escapeMenuCG.alpha = 1;
        escapeMenuCG.blocksRaycasts = true;
        input.EnableUIInputs();
    }

    public void HideEscapeScreen()
    {
        if (escapeMenuCG == null)
            return;

        escapeMenuCG.alpha = 0;
        escapeMenuCG.blocksRaycasts = false;
        input.EnableGameplayInputs();
        isShowingEscapeScreen = false;
    }

    public void SetKeypadShown() =>
        isShowingKeypad = true;

    public void SetKeypadHidden() =>
        isShowingKeypad = false;

    public void CloseGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowCredits()
    {
        panelMainMenu.SetActive(false);
        panelCredits.SetActive(true);
    }

    public void BackToMenu()
    {
        panelCredits.SetActive(false);
        panelMainMenu.SetActive(true);
    }
}
