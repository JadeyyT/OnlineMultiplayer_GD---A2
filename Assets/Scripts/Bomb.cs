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
   public Tilemap obstacleTilemap; 


    public PlayerMovement owner;

    public int explosionRange = 1;
    private float fuseTime = 2f;
    private float explosionEffectLifetime = 1.5f;
    public GameObject[] powerUpPrefabs;  
[Range(0f, 1f)] public float powerUpSpawnChance = 0.3f;  


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
        Vector3 worldPos = softBlockTilemap.GetCellCenterWorld(tilePos);

        // If obstacle exists, destroy it first (even on hard tiles)
        bool obstacleDestroyed = false;
        if (obstacleTilemap.HasTile(tilePos))
        {
            obstacleTilemap.SetTile(tilePos, null);
            Debug.Log("Obstacle destroyed at: " + tilePos);

            GameObject explosion = Instantiate(
                i == explosionRange ? explosionEndPrefab : explosionBodyPrefab,
                worldPos,
                Quaternion.identity
            );
            if (i == explosionRange)
                RotateEnd(explosion, direction);

            obstacleDestroyed = true;
        }

        //If there's still a hard tile and no obstacle was just cleared, stop explosion
        if (hardBlockTilemap.HasTile(tilePos) && !obstacleDestroyed)
        {
            break;
        }

        // Skip soft tile logic if we already destroyed an obstacle at this spot
        if (!obstacleDestroyed && softBlockTilemap.HasTile(tilePos))
        {
            softBlockTilemap.SetTile(tilePos, null);
            SpawnPowerUp(tilePos);

            GameObject explosion = Instantiate(
                explosionEndPrefab,
                worldPos,
                Quaternion.identity
            );
            RotateEnd(explosion, direction);
            break;
        }

        // Empty: just show explosion
        if (!obstacleDestroyed)
        {
            GameObject part = Instantiate(
                i == explosionRange ? explosionEndPrefab : explosionBodyPrefab,
                worldPos,
                Quaternion.identity
            );

            if (i == explosionRange)
                RotateEnd(part, direction);
        }
    }
}


private void RotateEnd(GameObject part, Vector2 direction)
{
    float angle = direction == Vector2.up ? 90f :
                  direction == Vector2.right ? 0f :
                  direction == Vector2.down ? -90f : 180f;

    part.transform.rotation = Quaternion.Euler(0f, 0f, angle);
}
private void SpawnPowerUp(Vector3Int cell)
{
    if (powerUpPrefabs.Length == 0 || Random.value > powerUpSpawnChance)
        return;

    int index = Random.Range(0, powerUpPrefabs.Length);
    Vector3 spawnPos = softBlockTilemap.GetCellCenterWorld(cell);
    Instantiate(powerUpPrefabs[index], spawnPos, Quaternion.identity);
}


}
