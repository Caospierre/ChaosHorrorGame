using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoodMonster : MonoBehaviour, IInteractable
{
    [SerializeField] private int candyProvided = 1;
    private bool candyGiven = false;

    public string GetPrompt()
    {
        if (!candyGiven)
        {
            return "Press (e) to talk";
        }
        else
        {
            return "I've already given you candy";
        }
    }

    public void Interact(GameObject interactor)
    {
        if (candyGiven) return; // Only give candy once

        if (interactor.TryGetComponent(out CandyController controller))
        {
            //controller.AddCandy(candyProvided);
            HotelLayoutManager.Instance.AddCandy(candyProvided);
            candyGiven = true;
        }
    }
}
