using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableElevator : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private GameObject elevatorContainer;
    private bool isVisible = false;

    private InputController inputController;

    private void Start()
    {
        // Ensure the UI starts hidden
        elevatorContainer.SetActive(false);

        // Find the active input controller in the scene
        inputController = FindFirstObjectByType<InputController>();

        if (inputController != null)
        {
            inputController.ClickPressed += HandleClick;
            inputController.CancelPressed += HandleCancel;
            inputController.ClosePressed += HandleCancel;
            inputController.EscapePressed += HandleCancel;
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
        isVisible = !isVisible;
        inputController.EnableUIInputs();
        elevatorContainer.SetActive(isVisible);
    }

    public void HideUI()
    {
        if (!isVisible) return;

        isVisible = false;
        inputController.EnableGameplayInputs();
        elevatorContainer.SetActive(false);
    }

    private void HandleCancel()
    {
        if (isVisible)
        {
            HideUI();
        }
    }

    private void HandleClick()
    {
        if (!isVisible) return;

        StartCoroutine(DeferredClickCheck());
    }

    private System.Collections.IEnumerator DeferredClickCheck()
    {
        // Wait one frame so Unity updates the EventSystem
        yield return null;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HideUI();
        }
    }

    public string GetPrompt()
    {
        return "Press (e) to use elevator";
    }
}