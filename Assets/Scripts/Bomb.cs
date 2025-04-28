using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explosionDelay = 2f;
    public GameObject explosionEffectPrefab; 

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        // Spawn the explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 0.5f); 
        }

        Destroy(gameObject);
    }
}
