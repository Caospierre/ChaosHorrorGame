using UnityEngine;

// Has on trigger collider that activates once
// Causes attack on player resulting in -1 candy or death

[RequireComponent(typeof(Collider))]
public class LurkingMonster : MonoBehaviour // 'ForcedMonster' RoomInteractionType
{
    [SerializeField] private GameObject destroyedComponent;
    [SerializeField] private Animator animator;

    private Collider col;

    private bool triggered = false; // Ensure we can't get duplicate triggers -- Just to be safe
    private InputController playerInput = null;

    private void Awake()
    {
        // Force isTrigger
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (other.TryGetComponent(out InputController player))
        {
            triggered = true;
            TriggerAttack(player);

            // If we have an animator it will handle destruction in the callback
            if (animator == null)
            {
                if (destroyedComponent != null)
                    Destroy(destroyedComponent);

                Destroy(this); // Remove this script
            }
        }
    }

    private void TriggerAttack(InputController player)
    {
        Debug.Log("You were attacked by a lurking monster");
        player.EnableUIInputs();
        playerInput = player;

        if (animator == null)
        {
            //UIManager.Instance.ShowMonsterAttack(); <- Needed
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    public void AnimationCallback() // Called from relay on animation end
    {
        //UIManager.Instance.ShowMonsterAttack(); <- Needed

        HotelLayoutManager.Instance.RemoveCandy();
        playerInput.EnableGameplayInputs();

        if (destroyedComponent != null)
            Destroy(destroyedComponent);

        Destroy(this); // Remove this script
    }
}
