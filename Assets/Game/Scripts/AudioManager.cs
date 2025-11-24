using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip loadingMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField] private bool loopMusic = true;
    [SerializeField] private float fadeDuration = 1f;

    private AudioSource audioSource;
    private bool isFading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = loopMusic;
        audioSource.volume = musicVolume;
        audioSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            PlayMusic(menuMusic);
        }
        else if (sceneName == "LoadingScreen")
        {
            if (loadingMusic != null)
            {
                PlayMusic(loadingMusic);
            }
        }
        else if (sceneName == "GameScene" || sceneName == "Playground")
        {
            if (gameMusic != null)
            {
                PlayMusic(gameMusic);
            }
            else
            {
                StopMusic();
            }
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayMusicWithFade(AudioClip clip)
    {
        if (clip == null) return;

        if (!isFading)
        {
            StartCoroutine(FadeMusic(clip));
        }
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSource.volume = musicVolume;
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip newClip)
    {
        isFading = true;
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        while (audioSource.volume < musicVolume)
        {
            audioSource.volume += musicVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = musicVolume;
        isFading = false;
    }
}
