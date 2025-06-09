using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
   

public GameObject floatingHeartPrefab;
    public GameObject floatingBombPrefab; 
public GameObject shieldEffectPrefab;
    public GameObject activeShieldEffect;
public GameObject speedTailPrefab;
    private GameObject activeSpeedTail; 
public Transform speedTailAnchor;
public GameObject shieldEffectObject;
public GameObject speedEffectObject; 
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
                StartCoroutine(SpawnFloatingWorldText("+1 ❤️", Color.red));
                SpawnFloatingIcon(floatingHeartPrefab);
            }
            break;

        case PowerUp.PowerUpType.ExtraBomb:
            bombLimit++;
            bombsRemaining++;
            UpdateBombsText();
            StartCoroutine(SpawnFloatingWorldText("+1 💣", Color.yellow));
            SpawnFloatingIcon(floatingBombPrefab);
            break;

        case PowerUp.PowerUpType.SpeedBoost:
            StartCoroutine(SpeedBoost());
            break;

        case PowerUp.PowerUpType.Shield:
            StartCoroutine(ShieldRoutine());
            break;
    }
}

private IEnumerator SpawnFloatingWorldText(string text, Color color)
{
    GameObject go = new GameObject("FloatingText");
    go.transform.position = transform.position + new Vector3(0, 0.7f, 0);

    TextMeshPro textMesh = go.AddComponent<TextMeshPro>();
    textMesh.text = text;
    textMesh.fontSize = 2;
    textMesh.color = color;
    textMesh.alignment = TextAlignmentOptions.Center;
    textMesh.sortingOrder = 100;

    float duration = 1f;
    float elapsed = 0f;
    Vector3 startPos = go.transform.position;

    while (elapsed < duration)
    {
        float t = elapsed / duration;
        go.transform.position = startPos + Vector3.up * t * 1f;
        textMesh.alpha = Mathf.Lerp(1f, 0f, t);
        elapsed += Time.deltaTime;
        yield return null;
    }

    Destroy(go);
}

private void SpawnFloatingIcon(GameObject prefab)
{
    if (prefab == null) return;

    GameObject icon = Instantiate(prefab, transform.position + new Vector3(0, 0.6f, 0), Quaternion.identity);

    StartCoroutine(FadeAndDestroy(icon, 1.2f));
}

private IEnumerator FadeAndDestroy(GameObject obj, float duration)
{
    float elapsed = 0f;
    Vector3 startPos = obj.transform.position;
    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
    if (sr == null) sr = obj.GetComponentInChildren<SpriteRenderer>();

    Color startColor = sr != null ? sr.color : Color.white;

    while (elapsed < duration)
    {
        float t = elapsed / duration;
        obj.transform.position = startPos + Vector3.up * t * 0.5f;

        if (sr != null)
        {
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    Destroy(obj);
}

   private IEnumerator SpeedBoost()
{
    originalSpeed = moveSpeed;
    moveSpeed *= 1.5f;

    if (speedEffectObject != null)
    {
        speedEffectObject.SetActive(true);
        speedEffectObject.transform.localScale = Vector3.one;
        SetSpeedEffectAlpha(1f);
    }

    yield return new WaitForSeconds(3.5f); // Stay normal for 3.5 seconds

    // Pulse animation for final 1.5s
    float pulseDuration = 1.5f;
    float elapsed = 0f;

    while (elapsed < pulseDuration)
    {
        float t = Mathf.PingPong(elapsed * 4f, 1f);
        float scale = Mathf.Lerp(1f, 1.2f, t);
        float alpha = Mathf.Lerp(1f, 0.4f, t);

        if (speedEffectObject != null)
        {
            speedEffectObject.transform.localScale = Vector3.one * scale;
            SetSpeedEffectAlpha(alpha);
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    // Cleanup
    if (speedEffectObject != null)
    {
        speedEffectObject.SetActive(false);
    }

    moveSpeed = originalSpeed;
}
private void SetSpeedEffectAlpha(float alpha)
{
    SpriteRenderer sr = speedEffectObject.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }
}


private IEnumerator ShieldRoutine()
{
    isShielded = true;

    if (shieldEffectObject != null)
    {
        shieldEffectObject.SetActive(true);

        // Reset scale and opacity
        shieldEffectObject.transform.localScale = Vector3.one;
        SetShieldAlpha(1f);
    }

    yield return new WaitForSeconds(3.5f); // Normal phase

    // Start pulsing for the last 1.5 seconds
    float pulseDuration = 1.5f;
    float elapsed = 0f;

    while (elapsed < pulseDuration)
    {
        float t = Mathf.PingPong(elapsed * 4f, 1f); // Oscillate 4 times over 1.5s
        float scale = Mathf.Lerp(1f, 1.2f, t); // Scale between 1x and 1.2x
        float alpha = Mathf.Lerp(1f, 0.4f, t); // Fade between full and semi-transparent

        if (shieldEffectObject != null)
        {
            shieldEffectObject.transform.localScale = Vector3.one * scale;
            SetShieldAlpha(alpha);
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    // End shield
    if (shieldEffectObject != null)
    {
        shieldEffectObject.SetActive(false);
    }

    isShielded = false;
}
private void SetShieldAlpha(float alpha)
{
    SpriteRenderer sr = shieldEffectObject.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }
}




}
