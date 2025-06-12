using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TMP_Text livesText;
    public TMP_Text countdownText;
    public PlayerMovement[] playersToFreeze;

    public AudioSource sfxSource;
    public AudioClip countdownBeep;
    public AudioClip goSound;

    private int lastLives = -1;
    private Coroutine livesAnimRoutine;

    void Start()
    {
        if (countdownText != null)
            StartCoroutine(CountdownBeforeStart());
    }

    private IEnumerator CountdownBeforeStart()
    {
        foreach (var player in playersToFreeze)
            player.enabled = false;

        countdownText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            PlaySFX(countdownBeep);
            yield return StartCoroutine(AnimateCountdownPunch(countdownText));
            yield return new WaitForSeconds(0.3f);
        }

        countdownText.text = "GO!";
        PlaySFX(goSound);
        yield return StartCoroutine(AnimateCountdownPunch(countdownText));
        yield return new WaitForSeconds(0.5f);

        countdownText.gameObject.SetActive(false);

        foreach (var player in playersToFreeze)
            player.enabled = true;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    private IEnumerator AnimateCountdownPunch(TMP_Text text)
    {
        Vector3 originalScale = text.transform.localScale;
        Color originalColor = text.color;
        Color punchColor = new Color(1f, 0.6f, 0.2f);

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, 1.6f, Mathf.Sin(t * Mathf.PI));
            text.transform.localScale = originalScale * scale;
            text.color = Color.Lerp(originalColor, punchColor, Mathf.PingPong(elapsed * 3f, 1f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.transform.localScale = originalScale;
        text.color = originalColor;
    }

    void Update()
    {
        if (playerHealth != null && livesText != null)
        {
            int currentLives = playerHealth.lives;
            livesText.text = "Lives: " + currentLives;

            if (currentLives != lastLives)
            {
                if (livesAnimRoutine != null) StopCoroutine(livesAnimRoutine);
                livesAnimRoutine = StartCoroutine(AnimateTextPunch(livesText, Color.red));
                lastLives = currentLives;
            }
        }
    }

    private IEnumerator AnimateTextPunch(TMP_Text text, Color changeColor)
    {
        Vector3 originalScale = text.transform.localScale;
        Color originalColor = text.color;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            text.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.4f, t);
            text.color = Color.Lerp(originalColor, changeColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            text.transform.localScale = Vector3.Lerp(originalScale * 1.4f, originalScale, t);
            text.color = Color.Lerp(changeColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.transform.localScale = originalScale;
        text.color = originalColor;
    }
}
