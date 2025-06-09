using UnityEngine;

public class ExplosionPart : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f);
    }

  private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null && movement.isShielded)
        {
            Debug.Log("Player is shielded — explosion ignored.");
            return; // Don't apply damage
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage();
        }
    }
}

}
