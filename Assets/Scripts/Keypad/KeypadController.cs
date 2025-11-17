using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class KeypadController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Image[] failIndicators; // 3 UI images for failed attempts

    [Header("Keypad Settings")]
    [SerializeField] private string correctCode; // Set by HotelLayoutManager

    [SerializeField] private string currentInput = ""; // Tracks what player types
    private int failedAttempts = 0;   // Number of failed tries

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] numberKeyClips;

    private const int MAX_ATTEMPTS = 3;
    private int codeLength;

    private readonly Color failColor = Color.red; // Fail color
    private readonly Color successColor = Color.green; // Success color
    private readonly Color emptyColor = Color.gray; // Empty color

    [Header("Events")]
    public UnityEvent OnVictory;
    public UnityEvent OnDefeat;

    private InteractableDoorKeypad doorKeypad;

    private InputController input = null;

    private void Start()
    {
        ResetInput();
        //ResetFailIndicators();
        SetFailIndicators();
        correctCode = HotelLayoutManager.Instance ? HotelLayoutManager.Instance.Keycode : "";
        codeLength = correctCode.Length;

        // Find the door keypad component in the scene
        doorKeypad = FindFirstObjectByType<InteractableDoorKeypad>();
        if (doorKeypad == null)
        {
            Debug.LogWarning("[Keypad] InteractableDoorKeypad not found in scene!");
        }

        // Connect to UIManager finish screen
        if (UIManager.Instance != null)
        {
            OnVictory.AddListener(UIManager.Instance.ShowVictory);
            OnDefeat.AddListener(UIManager.Instance.ShowDefeat);
        }
        else
        {
            Debug.LogWarning("[Keypad] UIManager instance not found!");
        }   
    }

    public void Show()
    {
        if (input == null)
        {
            input = FindFirstObjectByType<InputController>();
        }

        input.KeypadPressed += OnButtonPressed;
        input.EscapePressed += Hide;
        input.EnableUIInputs();
        gameObject.SetActive(true);

        UIManager.Instance?.SetKeypadShown();
    }

    public void Hide()
    {
        if (input != null)
        {
            input.KeypadPressed -= OnButtonPressed;
            input.EscapePressed -= Hide;
            input.EnableGameplayInputs();
        }

        gameObject.SetActive(false);
        UIManager.Instance?.SetKeypadHidden();
    }

    private void ResetFailIndicators()
    {
        // Grey color should already be set in editor, but safety reset if needed
        foreach (var indicator in failIndicators)
        {
            if (indicator != null)
                indicator.color = Color.gray;
        }
    }

    private void SetFailIndicators()
    {
        if (HotelLayoutManager.Instance != null)
            failedAttempts = HotelLayoutManager.Instance.KeypadFails;

        int index = 0;
        foreach (var indicator in failIndicators)
        {
            index++;
            if (index <= failedAttempts)
                indicator.color = Color.red;
            else
                indicator.color = Color.gray;
        }
    }

    private void PlayKeySound(string value)
    {
        if (SoundManager.Instance == null)
            return;

        if (int.TryParse(value, out int keyNumber))
        {
            if (numberKeyClips != null && keyNumber >= 0 && keyNumber < numberKeyClips.Length)
            {
                var clip = numberKeyClips[keyNumber];
                if (clip != null)
                    SoundManager.Instance.PlaySFX(clip);
            }
            return;
        }
    }

    

    public void OnButtonPressed(string value)
    {
        //Debug.Log($"[Keypad] Button pressed: {value}");
        PlayKeySound(value);

        if (failedAttempts >= MAX_ATTEMPTS)
        {
            //Debug.LogWarning("[Keypad] Input disabled. Maximum attempts reached.");
            return;
        }

        switch (value)
        {
            case "Clear":
                ResetInput();
                break;

            case "Backspace":
                if (currentInput.Length > 0)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateDisplay();
                break;

            case "Enter":
            case "Submit":
                SubmitCode();
                break;

            default:
                if (currentInput.Length < codeLength)
                {
                    currentInput += value;
                    UpdateDisplay();
                }
                break;
        }
    }

    public void SubmitCode()
    {
        //Debug.Log($"[Keypad] Submitting code: '{currentInput}' ({currentInput.Length}/{CODE_LENGTH})");

        if (currentInput.Length < codeLength)
        {
            displayText.text = "Incomplete";
            //Debug.LogWarning("[Keypad] Input too short.");
            Invoke(nameof(ResetInput), 1f);
            return;
        }

        if (currentInput == correctCode)
        {
            displayText.text = "Correct";
            //Debug.Log("[Keypad] Access granted!");

            // Turn the first available grey indicator green
            foreach (var indicator in failIndicators)
            {
                if (indicator != null && indicator.color == emptyColor)
                {
                    indicator.color = successColor;
                    //Debug.Log("[Keypad] Victory Condition.");
                    break;
                }
            }

            // Hide the keypad before showing finish screen
            if (doorKeypad != null)
                doorKeypad.HideKeypad();

            OnVictory.Invoke();
        }
        else
        {
            failedAttempts++;
            HotelLayoutManager.Instance.AddKeypadFail();
            displayText.text = "Incorrect";
            //Debug.Log($"[Keypad] Incorrect code. Attempt {failedAttempts}/{MAX_ATTEMPTS}");

            // Turn the next fail indicator red
            if (failedAttempts - 1 < failIndicators.Length && failIndicators[failedAttempts - 1] != null)
                failIndicators[failedAttempts - 1].color = failColor;

            if (failedAttempts >= MAX_ATTEMPTS)
            {
                //Debug.Log("[Keypad] Access denied. Maximum attempts reached.");
                
                // Hide the keypad before showing finish screen
                if (doorKeypad != null)
                    doorKeypad.HideKeypad();
                Hide();
                OnDefeat.Invoke();
            }
            else
            {
                Invoke(nameof(ResetInput), 1f);
            }
        }
    }

    private void ResetInput()
    {
        currentInput = "";
        if (displayText != null)
            displayText.text = "CLEARED";
        //Debug.Log("[Keypad] Input buffer cleared.");

        Invoke(nameof(ResetMask), 0.5f); // Small delay before re-masking
    }

    private void ResetMask()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
            displayText.text = currentInput.PadRight(codeLength, '*');
    }
    private void OnDestroy()
{
    if (UIManager.Instance != null)
    {
        OnVictory.RemoveListener(UIManager.Instance.ShowVictory);
        OnDefeat.RemoveListener(UIManager.Instance.ShowDefeat);
    }
}
}
