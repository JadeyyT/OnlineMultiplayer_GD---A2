using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed (used for smooth move)
    public float tileSize = 1f; // How big each tile is (usually 1 unit)

    public GameObject bombPrefab; // Drag your Bomb prefab here

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 targetPosition;
    private bool isMoving = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPosition = rb.position; // Start at your current tile
    }

    private void Update()
    {
        if (!isMoving)
        {
            if (moveInput != Vector2.zero)
            {
                Vector2 newTargetPos = targetPosition + new Vector2(moveInput.x * tileSize, moveInput.y * tileSize);

                // Set the new target position
                targetPosition = newTargetPos;
                isMoving = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.position = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

            // If we have reached the target tile
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

            // Only allow pure cardinal directions (up/down/left/right, not diagonals)
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
    Vector2 placePosition = new Vector2(Mathf.Round(targetPosition.x), Mathf.Round(targetPosition.y));
    Instantiate(bombPrefab, placePosition, Quaternion.identity);
}

}
