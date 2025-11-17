using UnityEngine;

public class CandyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private int candyProvided = 1;

    public string GetPrompt()
    {
        return "Press (e) to pickup candy";
    }

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CandyController controller))
        {
            //controller.AddCandy(candyProvided);
            HotelLayoutManager.Instance.AddCandy(candyProvided);

            Destroy(gameObject); // Destroy this object
        }
    }
}
