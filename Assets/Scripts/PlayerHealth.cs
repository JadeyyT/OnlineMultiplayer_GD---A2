using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int lives = 3;

    public void TakeDamage()
    {
        lives--;

        Debug.Log("Player hit! Lives left: " + lives);

        if (lives <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died.");
        gameObject.SetActive(false); // or destroy, or respawn
    }
}
