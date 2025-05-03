using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.Tilemaps;
using Mirror;

public class PlayerMovement : NetworkBehaviour

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


    public Tilemap groundTilemap;
    public Tilemap softBlockTilemap;
    public Tilemap hardBlockTilemap;
    public Tilemap obstacleTilemap;
    public Tilemap boundaryTilemap;

    private ParticleSystem trail;
public GameObject impactEffectPrefab;

private bool isWobbling = false;
    private Vector3 originalLocalPosition;

    public AudioSource moveSoundSource;
public AudioClip moveSoundClip;

private bool wasMoving = false; // To track changes


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is not attached to the player object.");
            return;
        }


        // Automatically assign tilemaps from the scene
        groundTilemap = GameObject.Find("GroundTilemap")?.GetComponent<Tilemap>();
        softBlockTilemap = GameObject.Find("SoftBlockTilemap")?.GetComponent<Tilemap>();
        hardBlockTilemap = GameObject.Find("HardBlockTilemap")?.GetComponent<Tilemap>();
        obstacleTilemap = GameObject.Find("ObstacleTilemap")?.GetComponent<Tilemap>();
        boundaryTilemap = GameObject.Find("BoundaryTilemap")?.GetComponent<Tilemap>();

        // Debug to check if any tilemap is null
        if (groundTilemap == null) Debug.LogError("GroundTilemap not found!");
        if (softBlockTilemap == null) Debug.LogError("SoftBlockTilemap not found!");
        if (hardBlockTilemap == null) Debug.LogError("HardBlockTilemap not found!");
        if (obstacleTilemap == null) Debug.LogError("ObstacleTilemap not found!");
        if (boundaryTilemap == null) Debug.LogError("BoundaryTilemap not found!");

        // Ensure that the Rigidbody2D is correctly initialized
        if (!isLocalPlayer)
        {
            GetComponent<PlayerInput>().enabled = false;
            return;
        }

        // Calculate and set the target position based on the groundTilemap
        Vector3Int cell = groundTilemap.WorldToCell(transform.position);
        targetPosition = groundTilemap.GetCellCenterWorld(cell);

        // Set the initial position of the Rigidbody to the target position
        rb.position = targetPosition;

        // Initialize bomb count and text UI
        bombsRemaining = totalBombs;
        UpdateBombsText();

        // Set up the movement sound if audio source and clip are assigned
        if (moveSoundSource != null && moveSoundClip != null)
        {
            moveSoundSource.clip = moveSoundClip;
            moveSoundSource.loop = true;
        }

        // Optionally, initialize any other required logic, such as trail effects, etc.
        if (trail != null)
        {
            trail.Stop();
        }
    }


    public int GetBombsRemaining()
{
    return bombsRemaining;
}


    private void Update()
    
{  
    if (!isLocalPlayer) return;

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
    if (moveSoundSource != null)
    {
        if (isMoving && !wasMoving && !moveSoundSource.isPlaying)
            moveSoundSource.Play();
        else if (!isMoving && wasMoving && moveSoundSource.isPlaying)
            moveSoundSource.Stop();
    }

    wasMoving = isMoving;


       if (!isMoving && moveInput != Vector2.zero)
{
    Vector3Int currentCell = groundTilemap.WorldToCell(rb.position);
    Vector3Int nextCell = currentCell + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);

    // Prevent movement into hard blocks or obstacles or boundary
   if (obstacleTilemap.HasTile(nextCell) || boundaryTilemap.HasTile(nextCell))

{
    // Play impact sound
    AudioManager.Instance?.PlayObstacleBlocked();

    

    // Spawn impact effect
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
        if (!isLocalPlayer) return;

        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();

            // Normalize movement
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

            // Send the movement to the server
            CmdMovePlayer(moveInput);
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
            CmdMovePlayer(moveInput);  // Tell server to stop moving
        }
    }

    [Command]
    void CmdMovePlayer(Vector2 direction)
    {
        // Move the player on the server
        MovePlayer(direction);
    }

    // Server-side movement logic
    void MovePlayer(Vector2 direction)
    {
        // ✅ DO NOT check isLocalPlayer here
        Vector2 target = rb.position + direction * moveSpeed * Time.deltaTime;

        rb.MovePosition(target);

        // Send the updated position to all clients
        RpcMovePlayer((Vector3)target);
    }

    [ClientRpc]
    void RpcMovePlayer(Vector3 targetPosition)
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is null in RpcMovePlayer.");
            return;
        }

        // ✅ Corrected: Only move remote clients
        if (!isLocalPlayer)
        {
            rb.MovePosition(targetPosition);  // <- was rb.position = ... (use MovePosition for smoothness)
        }
    }



    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        
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

    private void UpdateBombsText()
    {
        if (bombsText != null)
            bombsText.text = "Bombs: " + bombsRemaining;
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
}
