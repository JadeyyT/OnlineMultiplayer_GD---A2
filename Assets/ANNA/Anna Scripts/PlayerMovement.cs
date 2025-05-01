using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
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


    private void Start()
    {
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
       if (!isMoving && moveInput != Vector2.zero)
{
    Vector3Int currentCell = groundTilemap.WorldToCell(rb.position);
    Vector3Int nextCell = currentCell + new Vector3Int((int)moveInput.x, (int)moveInput.y, 0);

    // Prevent movement into hard blocks or obstacles
    if (obstacleTilemap.HasTile(nextCell))
    {
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
}
