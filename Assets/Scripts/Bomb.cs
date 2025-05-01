using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
{
    public GameObject explosionEffectPrefab;
    public GameObject explosionCenterPrefab;
    public GameObject explosionBodyPrefab;
    public GameObject explosionEndPrefab;

    public Tilemap softBlockTilemap;
    public Tilemap hardBlockTilemap;
    public PlayerMovement owner;

    public int explosionRange = 1;
    private float fuseTime = 2f;
    private float explosionEffectLifetime = 1.5f;

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    private void Explode()
{
    Vector3Int originCell = softBlockTilemap.WorldToCell(transform.position);
    Vector3 centerWorldPos = softBlockTilemap.GetCellCenterWorld(originCell);

    // Destroy soft block under the bomb
    if (softBlockTilemap.HasTile(originCell))
    {
        softBlockTilemap.SetTile(originCell, null);
        Debug.Log("Soft tile destroyed under bomb at: " + originCell);
    }

    // Center explosion
    Instantiate(explosionCenterPrefab, centerWorldPos, Quaternion.identity);

    // Optional particle visual
    if (explosionEffectPrefab != null)
    {
        GameObject effect = Instantiate(explosionEffectPrefab, centerWorldPos, Quaternion.identity);
        Destroy(effect, explosionEffectLifetime);
    }

    // Spread explosion in 4 directions
    CreateExplosionInDirection(Vector2.up);
    CreateExplosionInDirection(Vector2.down);
    CreateExplosionInDirection(Vector2.left);
    CreateExplosionInDirection(Vector2.right);

    if (owner != null)
    {
        owner.OnBombExploded();
    }

    Destroy(gameObject);
}


    private void CreateExplosionInDirection(Vector2 direction)
    {
        Vector3Int originCell = softBlockTilemap.WorldToCell(transform.position);

        for (int i = 1; i <= explosionRange; i++)
        {
            Vector3Int tilePos = originCell + new Vector3Int((int)direction.x * i, (int)direction.y * i, 0);

            // Stop if hitting hard block
            if (hardBlockTilemap.HasTile(tilePos))
                break;

            // Destroy soft block and stop
            if (softBlockTilemap.HasTile(tilePos))
            {
                softBlockTilemap.SetTile(tilePos, null);
                break;
            }

            // Place explosion effect
            Vector3 worldPos = softBlockTilemap.GetCellCenterWorld(tilePos);
            GameObject prefabToUse = (i == explosionRange) ? explosionEndPrefab : explosionBodyPrefab;

            GameObject part = Instantiate(prefabToUse, worldPos, Quaternion.identity);

            //  rotation for end pieces
            if (prefabToUse == explosionEndPrefab)
            {
                float angle = direction == Vector2.up ? 90f :
                              direction == Vector2.right ? 0f :
                              direction == Vector2.down ? -90f : 180f;

                part.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}
