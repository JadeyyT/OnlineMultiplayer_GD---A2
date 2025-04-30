using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionEffectPrefab; // particle visual
    public GameObject explosionCenterPrefab;
    public GameObject explosionBodyPrefab;
    public GameObject explosionEndPrefab;

    public int explosionRange = 1;
    private float fuseTime = 2f;
    private float explosionEffectLifetime = 1.5f; // seconds

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    private void Explode()
    {
        Vector2 origin = RoundToGrid(transform.position);

        // Spawn center explosion sprite
        Instantiate(explosionCenterPrefab, origin, Quaternion.identity);

        // Spawn explosion particle effect 
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, origin, Quaternion.identity);
            Destroy(effect, explosionEffectLifetime); // Auto-destroy after set time
        }

        // 4 directions
        CreateExplosionInDirection(Vector2.up);
        CreateExplosionInDirection(Vector2.down);
        CreateExplosionInDirection(Vector2.left);
        CreateExplosionInDirection(Vector2.right);

        Destroy(gameObject);
    }

    private void CreateExplosionInDirection(Vector2 direction)
    {
        Vector2 origin = RoundToGrid(transform.position);

        for (int i = 1; i <= explosionRange; i++)
        {
            Vector2 spawnPos = origin + direction * i;

            GameObject prefabToUse = (i == explosionRange) ? explosionEndPrefab : explosionBodyPrefab;

            GameObject part = Instantiate(prefabToUse, spawnPos, Quaternion.identity);

            if (prefabToUse == explosionEndPrefab)
            {
                float angle = 0f;
                if (direction == Vector2.up) angle = 90f;
                else if (direction == Vector2.right) angle = 0f;
                else if (direction == Vector2.down) angle = -90f;
                else if (direction == Vector2.left) angle = 180f;

                part.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    private Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }
}
