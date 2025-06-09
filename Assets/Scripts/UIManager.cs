using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TMP_Text livesText;

    private int lastLives = -1;
    private Coroutine livesAnimRoutine;

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
