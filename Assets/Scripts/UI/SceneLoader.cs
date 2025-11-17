using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Settings")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private bool useLoadingScreen = false;

    private string _pendingSpawnId;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        if (useLoadingScreen)
            StartCoroutine(LoadSceneAsync(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex)
    {
        if (useLoadingScreen)
            StartCoroutine(LoadSceneAsync(sceneIndex));
        else
            SceneManager.LoadScene(sceneIndex);
    }

    public void LoadScene(string sceneName, string spawnId)
    {
        _pendingSpawnId = spawnId;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen)
            loadingScreen.SetActive(true);

        // Small delay to show loading screen
        yield return new WaitForSeconds(0.1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // Could update a loading bar here
            // float progress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        if (loadingScreen)
            loadingScreen.SetActive(false);
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        if (loadingScreen)
            loadingScreen.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            yield return null;
        }

        if (loadingScreen)
            loadingScreen.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if(string.IsNullOrEmpty(_pendingSpawnId)) return;

        SpawnPoint target = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None)
            .Where(spawn => spawn.Id == _pendingSpawnId)
            .FirstOrDefault();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        //Debug.Log($"Scene loaded: {scene.name}, moving player to spawn point: {_pendingSpawnId} (found: {target != null})");
        if (player != null && target != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();

            // Disable CharacterController to make the teleport work. Re-enable after.
            if (cc != null) cc.enabled = false; 
            player.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            if (cc != null) cc.enabled = true;
        }

        _pendingSpawnId = null;
    }
}
