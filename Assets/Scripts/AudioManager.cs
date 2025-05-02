using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip bombPlaceClip;
    public AudioClip explosionClip;
    public AudioClip playerHurtClip;
    public AudioClip obstacleBlockedClip;
     public AudioSource bubbleMoveSource;
public AudioClip bubbleMoveClip;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayBombPlace() => PlaySFX(bombPlaceClip);
    public void PlayExplosion() => PlaySFX(explosionClip);
    public void PlayPlayerHurt() => PlaySFX(playerHurtClip);
    public void PlayObstacleBlocked() => PlaySFX(obstacleBlockedClip);
   

public void PlayBubbleMove()
{
    if (bubbleMoveSource != null && !bubbleMoveSource.isPlaying)
    {
        bubbleMoveSource.clip = bubbleMoveClip;
        bubbleMoveSource.loop = true;
        bubbleMoveSource.Play();
    }
}

public void StopBubbleMove()
{
    if (bubbleMoveSource != null && bubbleMoveSource.isPlaying)
    {
        bubbleMoveSource.Stop();
    }
}

}
