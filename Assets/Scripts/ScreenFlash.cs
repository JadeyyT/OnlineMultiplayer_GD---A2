using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;

    private Image flashImage;
    private Color originalColor;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        flashImage = GetComponent<Image>();
        originalColor = flashImage.color;
    }

    public void Flash(float duration = 0.2f)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(duration));
    }

    private System.Collections.IEnumerator FlashRoutine(float duration)
    {
        // Flash in
        flashImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        yield return new WaitForSeconds(duration);
        // Fade out
        flashImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
