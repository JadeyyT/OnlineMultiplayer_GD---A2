using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public GameObject bombPrefab;

    public int bombLimit = 1;
    public int totalBombs = 10;

    private int activeBombs = 0;
    private int bombsRemaining;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector3 targetPosition;
    private bool isMoving = false;

    public TMP_Text bombsText;

    // New tilemap references
    public Tilemap groundTilemap;
    public Tilemap softBlockTilemap;
    public Tilemap hardBlockTilemap;
    public Tilemap obstacleTilemap;

    private ParticleSystem trail;
    public GameObject impactEffectPrefab;

    private bool isWobbling = false;
    private Vector3 originalLocalPosition;

    //power ups
    public int lives = 1;
    public bool isShielded = false;
    private float originalSpeed;


    private void Start()
    {
        trail = GetComponentInChildren<ParticleSystem>();

        rb = GetComponent<Rigidbody2D>();

        // Snap player to starting tile
        Vector3Int cell = groundTilemap.WorldToCell(transform.position);
        targetPosition = groundTilemap.GetCellCenterWorld(cell);
        rb.position = targetPosition;

        bombsRemaining = totalBombs;
        UpdateBombsText();
    }

    private void Update()

    {
        if (trail != null)
        {
            if (isMoving)
            {
                if (!trail.isPlaying) trail.Play();
                AudioManager.Instance?.PlayBubbleMove();
            }
            else
            {
                if (trail.isPlaying) trail.Stop();
                AudioManager.Instance?.StopBubbleMove();
            }
        }


        if (!isMoving && moveInput != Vector2.zero)
        {
            Vector3Int currentCell = groundTilemap.WorldToCell(rb.position);
            Vector3Int nextCell = currentCell + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);

            // Prevent movement into hard blocks or obstacles
            if (obstacleTilemap.HasTile(nextCell) || hardBlockTilemap.HasTile(nextCell))
            {
                AudioManager.Instance?.PlayObstacleBlocked();

                if (impactEffectPrefab != null)
                {
                    Vector3 impactPos = groundTilemap.GetCellCenterWorld(nextCell);
                    Instantiate(impactEffectPrefab, impactPos, Quaternion.identity);
                }

                StartCoroutine(Wobble());
                return;
            }



            targetPosition = groundTilemap.GetCellCenterWorld(nextCell);
            isMoving = true;
        }

    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.position = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                rb.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();

            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                moveInput.y = 0;
                moveInput.x = Mathf.Sign(moveInput.x);
            }
            else
            {
                moveInput.x = 0;
                moveInput.y = Mathf.Sign(moveInput.y);
            }
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlaceBomb();
        }
    }

    private void PlaceBomb()
    {
        if (activeBombs >= bombLimit || bombsRemaining <= 0)
            return;

        Vector3Int cell = groundTilemap.WorldToCell(rb.position);
        Vector3 placePosition = groundTilemap.GetCellCenterWorld(cell);

        GameObject bomb = Instantiate(bombPrefab, placePosition, Quaternion.identity);

        AudioManager.Instance?.PlayBombPlace();

        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.owner = this;
            bombScript.hardBlockTilemap = hardBlockTilemap;
            bombScript.softBlockTilemap = softBlockTilemap;
            bombScript.obstacleTilemap = obstacleTilemap;
        }

        activeBombs++;
        bombsRemaining--;
        UpdateBombsText();
    }

   private Coroutine bombTextAnimRoutine;

private void UpdateBombsText()
{
    if (bombsText != null)
    {
        bombsText.text = "Bombs: " + bombsRemaining;
        if (bombTextAnimRoutine != null) StopCoroutine(bombTextAnimRoutine);
        bombTextAnimRoutine = StartCoroutine(AnimateTextPunch(bombsText, Color.yellow));
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

    public void OnBombExploded()
    {
        activeBombs = Mathf.Max(0, activeBombs - 1);
    }

    public void SetBombLimit(int newLimit)
    {
        bombLimit = Mathf.Max(1, newLimit);
    }

    public void SetTotalBombs(int newTotal)
    {
        totalBombs = Mathf.Max(0, newTotal);
        bombsRemaining = totalBombs;
    }

    private IEnumerator Wobble()
    {
        if (isWobbling) yield break;

        isWobbling = true;
        originalLocalPosition = transform.localPosition;

        float wobbleTime = 0.2f;
        float elapsed = 0f;
        float strength = 0.05f;

        while (elapsed < wobbleTime)
        {
            float x = Mathf.Sin(elapsed * 40f) * strength;
            transform.localPosition = originalLocalPosition + new Vector3(x, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        isWobbling = false;
    }
    
    public void ApplyPowerUp(PowerUp.PowerUpType type)
{
    switch (type)
    {
        case PowerUp.PowerUpType.ExtraLife:
    PlayerHealth health = GetComponent<PlayerHealth>();
    if (health != null)
    {
        health.AddLife();
    }
    break;


     case PowerUp.PowerUpType.ExtraBomb:
    bombLimit++;
    bombsRemaining++; // Add an extra bomb to the pool
    Debug.Log($"Bomb limit increased to: {bombLimit}, Bombs remaining: {bombsRemaining}");
    UpdateBombsText();
    break;



        case PowerUp.PowerUpType.SpeedBoost:
            StartCoroutine(SpeedBoost());
            Debug.Log("Speed boost activated!");
            
            break;

        case PowerUp.PowerUpType.Shield:
            StartCoroutine(ShieldRoutine());
            Debug.Log("Shield activated!");
            
            break;
    }
}
private IEnumerator SpeedBoost()
{
    originalSpeed = moveSpeed;
    moveSpeed *= 1.5f;
    yield return new WaitForSeconds(5f); // last 5 seconds
    moveSpeed = originalSpeed;
}

private IEnumerator ShieldRoutine()
{
    isShielded = true;
    // Optional: enable visual cue for shield here
    yield return new WaitForSeconds(5f); // last 5 seconds
    isShielded = false;
}

}
