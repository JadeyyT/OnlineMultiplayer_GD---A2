using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
   public PlayerHealth playerHealth;
    public TMP_Text livesText; 

    void Update()
    {
        if (playerHealth != null && livesText != null)
        {
            livesText.text = "Lives: " + playerHealth.lives;
        }
    }
}
