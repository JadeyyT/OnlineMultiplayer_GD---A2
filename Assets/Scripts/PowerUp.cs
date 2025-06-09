using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { ExtraLife, ExtraBomb, SpeedBoost, Shield }
    public PowerUpType type;

    public float bounceHeight = 0.15f;
    public float bounceSpeed = 2f;

    public GameObject collectEffectPrefab;  // ← assign in Inspector
    public AudioClip collectSound;          // ← assign in Inspector

    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
    }

    private void Update()
    {
        float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = originalPosition + new Vector3(0f, newY, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.ApplyPowerUp(type);

            // 🔊 Play sound (if AudioManager is used)
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
                // or use AudioManager.Instance?.Play("PowerUpCollect");
            }

            // ✨ Spawn particle effect
           if (collectEffectPrefab != null)
{
    GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
    Destroy(effect, 2f); // optional fallback in case Unity's StopAction doesn't fire
}


            Destroy(gameObject);
        }
    }
}
