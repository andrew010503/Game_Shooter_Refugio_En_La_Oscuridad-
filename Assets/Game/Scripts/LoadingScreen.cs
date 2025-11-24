using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("Loading Settings")]
    [SerializeField] private string sceneToLoad = "GameScene";
    [SerializeField] private float minimumLoadTime = 2f;

    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private Image logo;

    [Header("Loading Messages")]
    [SerializeField]
    private string[] loadingMessages = new string[]
    {
        "Loading",
        "Preparing weapons",
        "Spawning enemies",
        "Almost ready"
    };

    [Header("Animation Settings")]
    [SerializeField] private bool animateLoadingDots = true;
    [SerializeField] private float dotAnimationSpeed = 0.5f;

    [Header("Logo Animation")]
    [SerializeField] private bool animateLogo = true;
    [SerializeField] private bool logoPulse = true;
    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private float pulseScale = 1.1f;

    private void Start()
    {
        if (animateLogo && logo != null)
        {
            StartCoroutine(AnimateLogo());
        }
        
        if (animateLoadingDots)
        {
            StartCoroutine(AnimateLoadingText());
        }
        
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator AnimateLoadingText()
    {
        int dots = 0;
        int messageIndex = 0;
        
        while (true)
        {
            if (loadingText != null && messageIndex < loadingMessages.Length)
            {
                string baseText = loadingMessages[messageIndex];
                loadingText.text = baseText + new string('.', dots);
            }
            
            dots = (dots + 1) % 4;
            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }

    private IEnumerator AnimateLogo()
    {
        if (logo == null) yield break;

        RectTransform logoRect = logo.rectTransform;
        Vector3 originalScale = logoRect.localScale;

        if (logoPulse)
        {
            StartCoroutine(PulseLogo(logoRect, originalScale));
        }
    }

    private IEnumerator PulseLogo(RectTransform logoRect, Vector3 originalScale)
    {
        while (true)
        {
            float elapsed = 0f;
            
            while (elapsed < pulseDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (pulseDuration / 2f);
                float scale = Mathf.Lerp(1f, pulseScale, Mathf.Sin(t * Mathf.PI));
                logoRect.localScale = originalScale * scale;
                yield return null;
            }
            
            logoRect.localScale = originalScale;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        float startTime = Time.time;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        int messageIndex = 0;
        float messageTimer = 0f;
        float messageInterval = minimumLoadTime / loadingMessages.Length;

        while (!operation.isDone)
        {
            messageTimer += Time.deltaTime;
            
            if (messageTimer >= messageInterval && messageIndex < loadingMessages.Length - 1)
            {
                messageIndex++;
                messageTimer = 0f;
            }

            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, progress, Time.deltaTime * 2f);
            }

            if (percentageText != null)
            {
                percentageText.text = $"{Mathf.RoundToInt(progressBar.value * 100)}%";
            }

            if (operation.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;

                if (elapsedTime >= minimumLoadTime)
                {
                    if (progressBar != null)
                    {
                        progressBar.value = 1f;
                    }

                    if (percentageText != null)
                    {
                        percentageText.text = "100%";
                    }

                    if (loadingText != null)
                    {
                        loadingText.text = "Ready!";
                    }

                    yield return new WaitForSeconds(0.5f);

                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
