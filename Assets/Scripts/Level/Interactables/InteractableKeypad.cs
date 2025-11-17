using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableDoorKeypad : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private KeypadController keypad;
    private bool isKeypadVisible = false;

    private InputController inputController;

    private void Start()
    {
        // Find the active input controller in the scene
        inputController = FindFirstObjectByType<InputController>();

        if (inputController != null)
        {
            inputController.ClickPressed += HandleClick;
            inputController.CancelPressed += HandleCancel;
            inputController.ClosePressed += HandleCancel;
            inputController.EscapePressed += HandleCancel;
        }
        else
        {
            //Debug.LogWarning("No InputController found in scene. Keypad cannot respond to input.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from input events to prevent memory leaks
        if (inputController != null)
        {
            inputController.ClickPressed -= HandleClick;
            inputController.CancelPressed -= HandleCancel;
            inputController.ClosePressed -= HandleCancel;
            inputController.EscapePressed -= HandleCancel;
        }
    }

    public void Interact(GameObject interactor)
    {
        isKeypadVisible = !isKeypadVisible;
        keypad.Show();

        //Debug.Log("Keypad " + (isKeypadVisible ? "opened" : "closed"));
    }

    public void HideKeypad()
    {
        if (!isKeypadVisible)
            return;

        isKeypadVisible = false;
        keypad.Hide();

        //Debug.Log("Keypad closed.");
    }

    private void HandleCancel()
    {
        if (isKeypadVisible)
            HideKeypad();
    }

    private void HandleClick()
    {
        if (!isKeypadVisible)
            return;

        StartCoroutine(DeferredClickCheck());
    }

    private System.Collections.IEnumerator DeferredClickCheck()
    {
        // Wait one frame so Unity updates the EventSystem
        yield return null;

        if (!EventSystem.current.IsPointerOverGameObject())
            HideKeypad();
    }

    public string GetPrompt()
    {
        return "Press E to use keypad";
    }
}