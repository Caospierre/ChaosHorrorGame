using UnityEngine;

// Depreciated since we are respawning the player every new scene change
public class CandyController : MonoBehaviour
{
    #region Depreciated
    private int currentCandy = 2;
    [SerializeField] private AudioClip addCandySFX;
    [SerializeField] private AudioClip removeCandySFX;

    private void Start()
    {
        //if (UIManager.Instance != null)
            //UIManager.Instance.UpdateCandyDisplay(currentCandy);
    }

    
    public void AddCandy(int amount = 1)
    {
        currentCandy += amount;

        UIManager.Instance.UpdateCandyDisplay(currentCandy);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFXSingle(addCandySFX);

        UIManager.Instance.UpdateCandyDisplay(currentCandy);
    }

    public void RemoveCandy(int amount = 1)
    {
        currentCandy -= amount;

        if (currentCandy < 0)
            UIManager.Instance.ShowCandyDefeat();
        
        if (SoundManager.Instance != null && removeCandySFX != null)
        {
            Debug.Log("Candy negative test");
            SoundManager.Instance.PlaySFXSingle(removeCandySFX);   
        }

        UIManager.Instance.UpdateCandyDisplay(currentCandy);
    }
    #endregion
}
