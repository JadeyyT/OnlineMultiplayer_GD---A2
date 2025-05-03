using UnityEngine;
using TMPro;
using Mirror;

public class UIManager : MonoBehaviour
{
    public TMP_Text livesText;
    public TMP_Text bombsText;

    private PlayerHealth localHealth;
    private PlayerMovement localMovement;

    private int lastLives = -1;
    private int lastBombs = -1;

    private void Update()
    {
        // Find and cache local player once
        if (localHealth == null || localMovement == null)
        {
            foreach (var health in FindObjectsOfType<PlayerHealth>())
            {
                if (health.isLocalPlayer)
                {
                    localHealth = health;
                    localMovement = health.GetComponent<PlayerMovement>();
                    break;
                }
            }
        }

        if (localHealth != null && livesText != null)
        {
            if (localHealth.lives != lastLives)
            {
                livesText.text = "Lives: " + localHealth.lives;
                lastLives = localHealth.lives;
            }
        }

        if (localMovement != null && bombsText != null)
        {
            int currentBombs = localMovement.GetBombsRemaining();
            if (currentBombs != lastBombs)
            {
                bombsText.text = "Bombs: " + currentBombs;
                lastBombs = currentBombs;
            }
        }
    }
}
