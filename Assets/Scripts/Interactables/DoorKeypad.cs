using UnityEngine;

public class DoorKeypadInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private KeypadController keypad;
    private bool isKeypadVisible = false;

    public void Interact(GameObject interactor)
    {
        isKeypadVisible = !isKeypadVisible;
        //keypadCanvas.SetActive(isKeypadVisible);

        if (isKeypadVisible)
            keypad.Show();
        else
            keypad.Hide();
            
        Debug.Log("Keypad " + (isKeypadVisible ? "opened" : "closed"));
    }

    public string GetPrompt()
    {
        return "Press E to use Keypad";
    }
}
