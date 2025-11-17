using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionZone : MonoBehaviour
{
    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponentInParent<IInteractable>();
        
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteracter player = other.GetComponent<PlayerInteracter>();
        if (player)
            player.SetInteractable(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteracter player = other.GetComponent<PlayerInteracter>();
        if (player)
            player.ClearInteractable(interactable);
        if (interactable is InteractableDoorKeypad keypad)
            keypad.HideKeypad();
    }
}
