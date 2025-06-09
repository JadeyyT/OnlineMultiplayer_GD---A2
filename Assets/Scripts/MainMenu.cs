using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject rulesPanel;
    public ParticleSystem backgroundBubbles;

    public AudioClip clickSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void PlayGame()
    {
        PlayClick();
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        PlayClick();
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void ToggleRulesPanel()
    {
        PlayClick();
        bool isActive = !rulesPanel.activeSelf;
        rulesPanel.SetActive(isActive);

        if (backgroundBubbles != null)
        {
            if (isActive)
                backgroundBubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                backgroundBubbles.Play();
        }
    }

    public void CloseRulesPanel()
    {
        PlayClick();
        rulesPanel.SetActive(false);

        if (backgroundBubbles != null)
        {
            backgroundBubbles.Play();
        }
    }
}
