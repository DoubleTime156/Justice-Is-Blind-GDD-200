using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // 1. SINGLETON PATTERN: Allows other scripts (like an enemy's death script) 
    // to find and interact with this manager easily and efficiently.
    public static MusicManager Instance { get; private set; }

    [SerializeField]
    public AudioClip calm;

    [SerializeField]
    public AudioClip chase;

    [SerializeField]
    public AudioSource audioSource;

    // --- New/Updated Fields ---
    [Tooltip("Time in seconds for the music to fade out when calming down.")]
    [SerializeField]
    private float fadeDuration = 2.0f;

    [Tooltip("Minimum time (in seconds) an enemy must be chasing before the music switches.")]
    [SerializeField]
    private float chaseDelayDuration = 0.5f;

    // 2. EFFICIENT TRACKING: Changed from GameObject[] to List<EnemyAI> for faster component access and reliable removal.
    private List<EnemyAI> activeEnemies;

    // The original fields are retained below
    private bool allEnemiesCalm = true;
    private float audioOrigin = 1f;
    private AudioClip currentClip;
    private Coroutine fadeCoroutine;
    private Coroutine chaseDelayCoroutine;

    // --- Initialization and Cleanup ---

    void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Optionally, DontDestroyOnLoad(gameObject); if the manager should persist across scenes
        }
    }

    void Start()
    {
        // 3. PERFORMANCE FIX: Find ALL EnemyAI components ONCE at start.
        EnemyAI[] initialEnemies = FindObjectsOfType<EnemyAI>();
        activeEnemies = new List<EnemyAI>(initialEnemies);

        audioSource.volume = audioOrigin;

        // Start with calm music
        audioSource.clip = calm;
        audioSource.loop = true;
        audioSource.Play();
        currentClip = calm;
    }

    // 4. PUBLIC ENEMY REMOVAL: This is the critical function that must be called
    // by an enemy when it is knocked out/destroyed.
    /// <summary>
    /// Called by an enemy object's destruction script to remove it from the tracked list.
    /// This keeps the enemy count accurate without slow FindGameObjectsWithTag calls.
    /// </summary>
    public void RemoveEnemy(EnemyAI enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    // --- Update Loop (Highly Optimized) ---

    void Update()
    {
        // The list is already up-to-date and reliable.
        if (activeEnemies.Count == 0)
        {
            // Stop any pending chase transition if all enemies are gone
            if (chaseDelayCoroutine != null)
            {
                StopCoroutine(chaseDelayCoroutine);
                chaseDelayCoroutine = null;
            }
            // Transition to calm if not already there
            if (currentClip != calm)
            {
                toggleToCalm();
            }
            return;
        }

        bool anyEnemyChasing = false;

        // Iterate through the managed list of EnemyAI components
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            EnemyAI ai = activeEnemies[i];

            // 5. CRASH FIX: Check for null reference (in case an enemy was destroyed without calling RemoveEnemy)
            if (ai == null)
            {
                // Remove the null entry for stability and continue checking
                activeEnemies.RemoveAt(i);
                i--; // Decrement index because we removed an element
                continue;
            }

            if (ai.IsChasing)
            {
                toggleToChase();
                anyEnemyChasing = true;
                break;
            }
        }

        // Original logic check
        if (!anyEnemyChasing)
        {
            // If we were waiting for the chase delay, cancel it immediately
            if (chaseDelayCoroutine != null)
            {
                StopCoroutine(chaseDelayCoroutine);
                chaseDelayCoroutine = null;
            }

            toggleToCalm();
        }
    }

    // --- Toggle Methods (Functionally Identical to Previous Stable Version) ---

    private void toggleToChase()
    {
        if (currentClip == chase) return;
        if (chaseDelayCoroutine != null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        chaseDelayCoroutine = StartCoroutine(ChaseDelayRoutine());
    }

    private IEnumerator ChaseDelayRoutine()
    {
        yield return new WaitForSeconds(chaseDelayDuration);

        // Check again if the music is STILL calm before switching (in case an enemy stopped chasing 
        // between the yield and the next instruction)
        if (currentClip == calm)
        {
            audioSource.clip = chase;
            audioSource.volume = audioOrigin;
            audioSource.Play();
            currentClip = chase;
        }
        chaseDelayCoroutine = null;
    }

    private void toggleToCalm()
    {
        if (currentClip == calm) return;

        if (currentClip == chase && fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeOutAndSwitch(calm));
        }
    }

    private IEnumerator FadeOutAndSwitch(AudioClip targetClip)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = targetClip;
        audioSource.volume = audioOrigin;
        audioSource.Play();
        currentClip = targetClip;

        fadeCoroutine = null;
    }
}