using UnityEngine;
using System.Collections;

public class BubblePlant : MonoBehaviour
{
    [Header("Bubble Setup")]
    public GameObject bubblePrefab;
    public Transform spawnPoint;

    [Header("Timing Settings")]
    public float minInterval = 1f;
    public float maxInterval = 2.5f;

    [Header("Burst Settings")]
    public int minBubblesPerBurst = 1;
    public int maxBubblesPerBurst = 2;

    [Header("Bubble Speed")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 2f;

    private void Start()
    {
        if (bubblePrefab != null && spawnPoint != null)
        {
            StartCoroutine(SpawnBubbles());
        }
        else
        {
            Debug.LogError("BubblePlant is missing bubblePrefab or spawnPoint assignment!");
        }
    }

    private IEnumerator SpawnBubbles()
    {
        while (true)
        {
            // Wait for a random interval between bursts
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // Decide how many bubbles to spawn this time
            int bubblesToSpawn = Random.Range(minBubblesPerBurst, maxBubblesPerBurst + 1);

            for (int i = 0; i < bubblesToSpawn; i++)
            {
                if (bubblePrefab == null) yield break;

                // Create the bubble at the spawn point
                GameObject bubble = Instantiate(bubblePrefab, spawnPoint.position, Quaternion.identity);

                // Give it a random speed
                Bubble bubbleScript = bubble.GetComponent<Bubble>();
                if (bubbleScript != null)
                {
                    bubbleScript.floatSpeed = Random.Range(minSpeed, maxSpeed);
                }

                // Optional: Small delay between bubbles in a burst
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
