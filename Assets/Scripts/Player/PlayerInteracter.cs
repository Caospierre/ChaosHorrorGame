using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteracter : MonoBehaviour
{
    private InputController inputController;
    private IInteractable current;

    private void Awake()
    {
        inputController = GetComponent<InputController>();
    }

    private void OnEnable()
    {
        inputController.InteractPressed += InteractPerformed;
    }
    
    private void OnDisable()
    {
        inputController.InteractPressed -= InteractPerformed;
    }

    private void InteractPerformed()
    {
        if ((current as Object) != null)
        {
            current.Interact(gameObject);
            UIManager.Instance.ClearInteractionPrompt();
        }
    }

    public void SetInteractable(IInteractable i)
    {
        current = i;
        UIManager.Instance.ShowInteractionPrompt(i.GetPrompt());
    }
    public void ClearInteractable(IInteractable i)
    {
        UIManager.Instance.ClearInteractionPrompt();
        if (current == i)
            current = null;
    }
}
