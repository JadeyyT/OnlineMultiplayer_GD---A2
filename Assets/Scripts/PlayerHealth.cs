using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int lives = 3;
    
    public void AddLife()
{
    lives++;
    Debug.Log("Extra life gained! Total lives: " + lives);
   

    // Optional: show UI feedback here (like update TMP_Text or play animation)
    
}


   public void TakeDamage()
    {
        lives--;

        AudioManager.Instance?.PlayPlayerHurt();

        Debug.Log("Player hit! Lives left: " + lives);

        // Flash screen
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
