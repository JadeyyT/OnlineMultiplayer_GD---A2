using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int lives = 3;

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
