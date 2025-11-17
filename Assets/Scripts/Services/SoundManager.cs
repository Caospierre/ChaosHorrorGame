using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioSource ambientMusicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip mainMenuTrack;
    [SerializeField] private AudioClip hotelAmbientTrack;
    [SerializeField] private AudioClip candyAddClip;
    [SerializeField] private AudioClip candyRemoveClip;
    [SerializeField] private AudioClip[] footstepClips;

    // Stores any audio clips by name. Can be scaled later.
    private readonly Dictionary<string, AudioClip> _audioClips = new();

    private void Awake()
    {
        // Singleton setup to persist between scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe and listen for scene changes
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (Instance == null)
            return;

        // -------------------------------
        // FIX: Corrected log text (was mislabeled as "Hotel Lobby Scene" for menu)
        // -------------------------------
        if (newScene.name == "Main Menu Scene")
        {
            Debug.Log("[SoundManager] Entered Main Menu Scene");
            StopAmbient();
            PlayMenuMusic(mainMenuTrack);
            return;
        }

        // Ambient shared between Lobby and Floor
        if (newScene.name == "Final_Lobby" || newScene.name == "Final_Floor")
        {
            Debug.Log($"[SoundManager] Entered {newScene.name} Scene — Ambient Track Playing");
            StopMenuMusic();
            PlayAmbient(hotelAmbientTrack);
            return;
        }

        // Fallback: any other scene stops both
        StopMenuMusic();
        StopAmbient();
    }

    private void Start()
    {
        if (Instance == null)
            return;

        // Initialize audio for the starting scene
        var sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Main Menu Scene")
        {
            Instance.PlayMenuMusic(mainMenuTrack);
        }
        else if (sceneName == "Lobby" || sceneName == "Floor")
        {
            Instance.PlayAmbient(hotelAmbientTrack);
        }
        else
        {
            StopMenuMusic();
            StopAmbient();
        }
    }

    //
    // REGISTRATION & MANAGEMENT
    //

    public void RegisterClip(string name, AudioClip clip)
    {
        if (string.IsNullOrEmpty(name) || clip == null)
            return;

        _audioClips[name] = clip; // Add or update
    }

    public void UnregisterClip(string name)
    {
        _audioClips.Remove(name);
    }

    //
    // MUSIC CONTROL
    //

    public void PlayMenuMusic(AudioClip clip)
    {
        if (menuMusicSource == null || clip == null)
            return;

        if (menuMusicSource.isPlaying && menuMusicSource.clip == clip)
            return;

        menuMusicSource.Stop();
        menuMusicSource.clip = clip;
        menuMusicSource.loop = true;
        menuMusicSource.Play();
    }

    public void StopMenuMusic()
    {
        if (menuMusicSource != null && menuMusicSource.isPlaying)
            menuMusicSource.Stop();
    }

    public void PlayAmbient(AudioClip clip)
    {
        if (ambientMusicSource == null || clip == null)
            return;

        if (ambientMusicSource.isPlaying && ambientMusicSource.clip == clip)
            return;

        ambientMusicSource.Stop();
        ambientMusicSource.clip = clip;
        ambientMusicSource.loop = true;
        ambientMusicSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientMusicSource != null && ambientMusicSource.isPlaying)
            ambientMusicSource.Stop();
    }

    //
    // SFX CONTROL
    //

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Stop();
    }

    public void PlaySFXSingle(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.Stop();
        sfxSource.clip = clip; // Assign new clip
        sfxSource.loop = false;
        sfxSource.Play();
    }

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0)
            return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length - 1)];

        footstepSource.Stop();
        footstepSource.clip = clip;
        footstepSource.loop = false;
        footstepSource.pitch = Random.Range(0.95f, 1.05f);
        footstepSource.Play();
    }

    public void StopFootstep()
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }

    public void PlayLoop(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.clip = clip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
    }

    public void StopLoop()
    {
        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
    
    public void PlayOneShotNonLoop(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
    public bool IsSfxPlaying(AudioClip clip)
    {
        return sfxSource != null && sfxSource.isPlaying && sfxSource.clip == clip;
    }

    public void PlayCandyAdd() => PlaySFXSingle(candyAddClip);
    public void PlayCandyRemove() => PlaySFXSingle(candyRemoveClip);
}
