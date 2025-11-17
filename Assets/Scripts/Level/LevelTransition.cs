using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string levelToLoad = "";
    [SerializeField] private string spawnId = "";

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag == "Player") && !string.IsNullOrEmpty(levelToLoad))
        {
            if(!string.IsNullOrEmpty(spawnId))
            {
                SceneLoader.Instance.LoadScene(levelToLoad, spawnId);
            }
            else
            {
                SceneLoader.Instance.LoadScene(levelToLoad);
            }
        }
    }
}
