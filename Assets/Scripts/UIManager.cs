using UnityEngine;
using TMPro;
using Mirror;

public class UIManager : MonoBehaviour
{
    public TMP_Text livesText;
    public TMP_Text bombsText;

    public GameObject deathPanel;
    public GameObject deathText;

    private PlayerHealth localHealth;
    private PlayerMovement localMovement;

    private int lastLives = -1;
    private int lastBombs = -1;
    private bool hasShownDeath = false;

    private void Update()
    {
        // Find the local player if not yet found
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

        // Update lives
        if (localHealth != null)
        {
            if (livesText != null && localHealth.lives != lastLives)
            {
                livesText.text = "Lives: " + localHealth.lives;
                lastLives = localHealth.lives;
            }

            // Show death screen once
            if (!hasShownDeath && localHealth.lives <= 0)
            {
                ShowDeathScreen();
                hasShownDeath = true;
            }
        }

        // Update bombs
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

    private void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        if (deathText != null)
            deathText.SetActive(true);
    }
}
