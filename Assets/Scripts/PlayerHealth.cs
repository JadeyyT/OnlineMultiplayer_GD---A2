using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int lives = 3;
    private PlayerMovement movement;
    private bool isInvincible = false;
    public float invincibilityTime = 0.5f; // Half a second of invincibility

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    public void AddLife()
    {
        lives++;
        Debug.Log("Extra life gained! Total lives: " + lives);
    }

    public void TakeDamage()
    {
        if (isInvincible || (movement != null && movement.isShielded)) return;

        lives--;
        isInvincible = true;

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
        else
        {
            StartCoroutine(InvincibilityCooldown());
        }
    }

    private IEnumerator InvincibilityCooldown()
    {
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player died.");
        gameObject.SetActive(false); 
    }
}
