using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float tileSize = 1f;
    public GameObject bombPrefab;

    public int bombLimit = 1;              // Max bombs at once
    public int totalBombs = 10;            // Total bombs available for the whole game

    private int activeBombs = 0;           // Currently placed bombs
    private int bombsRemaining;            // Internal tracker for bombs left

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 targetPosition;
    private bool isMoving = false;
    public TMP_Text bombsText; 


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = rb.position;

        bombsRemaining = totalBombs; 

        UpdateBombsText();

    }

    private void Update()
    {
        if (!isMoving && moveInput != Vector2.zero)
        {
            Vector2 newTargetPos = targetPosition + new Vector2(moveInput.x * tileSize, moveInput.y * tileSize);
            targetPosition = newTargetPos;
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

    Vector2 placePosition = new Vector2(Mathf.Round(targetPosition.x), Mathf.Round(targetPosition.y));
    GameObject bomb = Instantiate(bombPrefab, placePosition, Quaternion.identity);

    Bomb bombScript = bomb.GetComponent<Bomb>();
    if (bombScript != null)
    {
        bombScript.owner = this;
    }

    activeBombs++;
    bombsRemaining--;

    UpdateBombsText(); 
}
private void UpdateBombsText()
{
    if (bombsText != null)
    {
        bombsText.text = "Bombs: " + bombsRemaining;
    }
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
