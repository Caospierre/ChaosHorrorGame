using UnityEngine;

public class InteractableClueTelevision : MonoBehaviour, IInteractable, IClueOwner
{
    public bool isInteractable = true;
    public ClueData clue { get; private set; }

    public string GetPrompt()
    {
        return "Press (e) to watch";
    }

    public void Interact(GameObject interactor)
    {
        if (!isInteractable)
            return;
        
        isInteractable = false;
        UIManager.Instance.DisplayClueUI(this);
    }

    public void SetClue(ClueData data)
    {
        clue = data;
    }
}
