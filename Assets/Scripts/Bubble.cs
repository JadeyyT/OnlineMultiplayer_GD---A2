using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifespan = 5f;
    public float growDuration = 3f;

    public GameObject popEffectPrefab;

    private bool isPopping = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Invoke(nameof(SelfDestruct), lifespan);
    }

    void Update()
    {
        if (!isPopping)
        {
            transform.Translate(Vector2.up * floatSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPopping) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.ApplyBubbleGrow(growDuration);

            if (popEffectPrefab != null)
            {
                GameObject popFX = Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
                Destroy(popFX, 1f); // Auto-destroy the pop effect after 1 second
            }

            StartCoroutine(PopAndDestroy(true));
        }
    }

    private void SelfDestruct()
    {
        if (!isPopping)
        {
            if (popEffectPrefab != null)
            {
                GameObject popFX = Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
                Destroy(popFX, 1f); // Also destroy if bubble expires on its own
            }

            StartCoroutine(PopAndDestroy(false));
        }
    }

    private IEnumerator PopAndDestroy(bool isPlayerTriggered)
    {
        isPopping = true;

        if (isPlayerTriggered)
        {
            AudioManager.Instance?.PlayBubblePop();
        }

        float duration = 0.25f;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 midScale = startScale * 1.4f;
        Vector3 endScale = Vector3.zero;

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;

        // Step 1: Slight bounce
        float bounceTime = duration * 0.2f;
        while (elapsed < bounceTime)
        {
            float t = elapsed / bounceTime;
            transform.localScale = Vector3.Lerp(startScale, midScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Step 2: Shrink and fade
        float shrinkTime = duration - bounceTime;
        elapsed = 0f;
        while (elapsed < shrinkTime)
        {
            float t = elapsed / shrinkTime;
            transform.localScale = Vector3.Lerp(midScale, endScale, t);
            if (sr != null)
            {
                sr.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
