using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnHeight = 0f;

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float waveDifficultyMultiplier = 1.2f;
    [SerializeField] private int maxWaves = 99;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private UnityEngine.UI.Image warningOverlay;
    [SerializeField] private float textAnimationDuration = 0.5f;
    [SerializeField] private float textPulseScale = 1.3f;
    [SerializeField] private float warningDuration = 2f;

    [Header("Warning Flash Effect")]
    [SerializeField] private bool enableFlashEffect = true;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private int flashCount = 2;
    [SerializeField] private float flashSpeed = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] [Range(0f, 1f)] private float warningSoundVolume = 0.7f;
    [SerializeField] private bool loopWarningSound = false;
    [SerializeField] private float soundDuration = 2f;

    private int currentWave = 0;
    private int aliveEnemiesCount = 0;
    private int currentWaveEnemyCount;
    private int totalKills = 0;
    private AudioSource audioSource;

    private void Start()
    {
        currentWaveEnemyCount = enemiesPerWave;
        UpdateKillsUI();
        
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }

        if (warningOverlay != null)
        {
            warningOverlay.gameObject.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && warningSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        StartCoroutine(WaveController());
    }

    private IEnumerator WaveController()
    {
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(ShowWarning(currentWave + 1));
        StartWave();

        while (currentWave < maxWaves)
        {
            yield return new WaitUntil(() => aliveEnemiesCount == 0);

            yield return new WaitForSeconds(timeBetweenWaves);

            yield return StartCoroutine(ShowWarning(currentWave + 1));
            StartWave();
        }
    }

    private void StartWave()
    {
        currentWave++;
        UpdateWaveUI();

        if (enemyPrefab == null)
        {
            Debug.LogError("❌ ERROR: enemyPrefab es NULL. Asigna el prefab en el Inspector.");
            return;
        }

        for (int i = 0; i < currentWaveEnemyCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            pos.y = spawnHeight;

            GameObject newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            
            if (newEnemy == null)
            {
                Debug.LogError($"❌ Error: No se pudo crear el enemigo {i + 1}");
                continue;
            }

            aliveEnemiesCount++;

            EnemyHealth health = newEnemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.OnDeath += OnEnemyDeath;
            }
            else
            {
                Debug.LogWarning($"⚠️ El enemigo {newEnemy.name} no tiene EnemyHealth.");
                aliveEnemiesCount--;
            }
        }

        currentWaveEnemyCount = Mathf.CeilToInt(currentWaveEnemyCount * waveDifficultyMultiplier);
    }

    private void OnEnemyDeath()
    {
        aliveEnemiesCount--;
        totalKills++;
        UpdateKillsUI();
        StartCoroutine(AnimateKillsText());
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE {currentWave:D2}";
            StartCoroutine(AnimateWaveText());
        }
    }

    private IEnumerator ShowWarning(int nextWave)
    {
        if (warningText == null) yield break;

        warningText.text = $"¡WAVE {nextWave:D2}!";
        warningText.gameObject.SetActive(true);

        if (warningSound != null && audioSource != null)
        {
            audioSource.clip = warningSound;
            audioSource.volume = warningSoundVolume;
            audioSource.loop = loopWarningSound;
            audioSource.Play();

            if (!loopWarningSound && soundDuration > 0f)
            {
                StartCoroutine(StopAudioAfterDuration(soundDuration));
            }
        }

        if (enableFlashEffect && warningOverlay != null)
        {
            StartCoroutine(FlashScreen());
        }

        RectTransform warningRect = warningText.rectTransform;
        Vector3 originalScale = warningRect.localScale;
        Color originalColor = warningText.color;

        warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        warningRect.localScale = originalScale * 0.5f;

        float fadeDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            float scale = Mathf.Lerp(0.5f, 1.2f, t);
            warningRect.localScale = originalScale * scale;

            float alpha = Mathf.Lerp(0f, 1f, t);
            warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        yield return new WaitForSeconds(warningDuration - fadeDuration * 2f);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            float alpha = Mathf.Lerp(1f, 0f, t);
            warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        warningText.gameObject.SetActive(false);
        warningRect.localScale = originalScale;
        warningText.color = originalColor;
    }

    private IEnumerator FlashScreen()
    {
        if (warningOverlay == null) yield break;

        warningOverlay.gameObject.SetActive(true);
        Color targetColor = flashColor;

        for (int i = 0; i < flashCount; i++)
        {
            float elapsed = 0f;
            while (elapsed < flashSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashSpeed;
                float alpha = Mathf.Lerp(0f, targetColor.a, t);
                warningOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < flashSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashSpeed;
                float alpha = Mathf.Lerp(targetColor.a, 0f, t);
                warningOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
                yield return null;
            }
        }

        warningOverlay.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        warningOverlay.gameObject.SetActive(false);
    }

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator AnimateWaveText()
    {
        if (waveText == null) yield break;

        RectTransform textRect = waveText.rectTransform;
        Vector3 originalScale = textRect.localScale;
        Color originalColor = waveText.color;

        waveText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        textRect.localScale = originalScale * textPulseScale;

        float elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textAnimationDuration;

            float scale = Mathf.Lerp(textPulseScale, 1f, t);
            textRect.localScale = originalScale * scale;

            float alpha = Mathf.Lerp(0f, 1f, t);
            waveText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        textRect.localScale = originalScale;
        waveText.color = originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    private void UpdateKillsUI()
    {
        if (killsText != null)
        {
            killsText.text = $"{totalKills:D2}";
        }
    }

    private IEnumerator AnimateKillsText()
    {
        if (killsText == null) yield break;

        RectTransform textRect = killsText.rectTransform;
        Vector3 originalScale = textRect.localScale;

        float elapsed = 0f;
        float animDuration = 0.2f;
        
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;

            float scale = Mathf.Lerp(1.5f, 1f, t);
            textRect.localScale = originalScale * scale;

            yield return null;
        }

        textRect.localScale = originalScale;
    }
}
