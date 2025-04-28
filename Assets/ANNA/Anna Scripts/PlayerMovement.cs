using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject bombPrefab; 

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("PlaceBomb pressed!");
            PlaceBomb();
        }
    }

    private void PlaceBomb()
    {
        Vector2 placePosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
        Instantiate(bombPrefab, placePosition, Quaternion.identity);
    }
}
