using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor); // We can change this from GameObject if need

    string GetPrompt(); // Incase we want to display prompts later
}
