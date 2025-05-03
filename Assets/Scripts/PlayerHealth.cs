using UnityEngine;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int lives = 3;

    public void TakeDamage()
    {
        if (!isServer) return; // only the server reduces health

        lives--;

        AudioManager.Instance?.PlayPlayerHurt();
        Debug.Log("Player hit! Lives left: " + lives);

        if (ScreenFlash.Instance != null)
        {
            ScreenFlash.Instance.Flash();
        }

        if (lives <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died.");
        gameObject.SetActive(false);
    }
}
