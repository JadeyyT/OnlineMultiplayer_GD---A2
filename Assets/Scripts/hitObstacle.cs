using UnityEngine;

public class hitObstacle : MonoBehaviour
{
    public float lifetime = 0.5f;
    public float fadeDuration = 0.4f;
    public float scalePulseAmount = 0.1f;

    private SpriteRenderer sr;
    private float timer = 0f;
    private Color originalColor;
    private Vector3 originalScale;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Optional scale pulse
        float pulse = 1f + Mathf.Sin(timer * 20f) * scalePulseAmount;
        transform.localScale = originalScale * pulse;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
