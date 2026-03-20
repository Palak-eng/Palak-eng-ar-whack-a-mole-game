using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Fixed Portals / Spawn Positions")]
    public Portal[] portals;

    [Header("Different Mole Prefabs")]
    public GameObject[] molePrefabs;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public GameObject timeOverPanel;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip[] bgmTracks;

    [Header("Game Settings")]
    public int targetScore = 30;
    public float spawnDelay = 1.5f;
    public float gameDuration = 45f;

    [Header("AR")]
    public ARGameActivator arGameActivator;

    private int score = 0;
    private bool gameRunning = false;
    private bool moleActive  = false;
    private float currentTime;
    private int currentTrackIndex = 0;
    private bool isPaused    = false;
    private float pausedTime;
    private bool isRestarting = false;

    private int lastPortalIndex = -1;
    private int lastMoleIndex   = -1;

    private Coroutine spawnLoopCoroutine;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        score       = 0;
        currentTime = gameDuration;
        UpdateScoreUI();
        UpdateTimerUI();

        if (winPanel != null)      winPanel.SetActive(false);
        if (timeOverPanel != null) timeOverPanel.SetActive(false);

        HideAllPortals();
    }

    void Update()
    {
        if (!gameRunning || isPaused) return;

        currentTime -= Time.deltaTime;
        if (currentTime < 0) currentTime = 0;

        UpdateTimerUI();

        if (bgmSource != null && !bgmSource.isPlaying)
            PlayNextTrack();

        if (currentTime <= 0)
            TimeOver();
    }

    // ─────────────────────────────────────────────────────────────
    // BGM
    // ─────────────────────────────────────────────────────────────

    void StartBGM()
    {
        if (bgmSource == null || bgmTracks == null || bgmTracks.Length == 0) return;
        currentTrackIndex = 0;
        PlayCurrentTrack();
    }

    void PlayCurrentTrack()
    {
        if (bgmSource == null || bgmTracks == null || bgmTracks.Length == 0) return;
        if (bgmTracks[currentTrackIndex] == null)
        {
            Debug.LogWarning("BGM track " + currentTrackIndex + " not assigned!");
            return;
        }
        bgmSource.loop = false;
        bgmSource.clip = bgmTracks[currentTrackIndex];
        bgmSource.Play();
    }

    void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % bgmTracks.Length;
        PlayCurrentTrack();
    }

    void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    // ─────────────────────────────────────────────────────────────
    // Pause / Resume
    // ─────────────────────────────────────────────────────────────

    public bool IsPaused() => isPaused;

    public void HideAllPortals()
    {
        foreach (Portal portal in portals)
            if (portal != null)
                portal.HidePortal();
    }

    public void PauseGame()
    {
        if (!gameRunning || isPaused) return;

        isPaused   = true;
        pausedTime = currentTime;

        // FIX: Hide moles using Renderer instead of SetActive.
        // SetActive(false) permanently kills Unity coroutines — the mole's
        // PopRoutine/GoDownAndDestroy never resume, so moleActive stays true
        // forever and no new mole ever spawns after resume.
        // Toggling Renderer visibility keeps coroutines alive.
        foreach (Portal portal in portals)
        {
            if (portal != null && portal.currentMole != null)
                SetMoleVisible(portal.currentMole, false);
        }

        if (bgmSource != null) bgmSource.Pause();
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!gameRunning || !isPaused || isRestarting) return;

        isPaused    = false;
        currentTime = pausedTime;

        // Re-show mole renderers
        foreach (Portal portal in portals)
        {
            if (portal != null && portal.currentMole != null)
                SetMoleVisible(portal.currentMole, true);
        }

        if (bgmSource != null) bgmSource.UnPause();
        Debug.Log("Game Resumed");
    }

    // Toggle all Renderers on the mole without using SetActive
    void SetMoleVisible(GameObject mole, bool visible)
    {
        foreach (Renderer r in mole.GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }

    // ─────────────────────────────────────────────────────────────
    // Game Flow
    // ─────────────────────────────────────────────────────────────

    public void StartGame()
    {
        if (gameRunning) return;

        score           = 0;
        currentTime     = gameDuration;
        gameRunning     = true;
        moleActive      = false;
        isPaused        = false;
        isRestarting    = false;
        lastPortalIndex = -1;
        lastMoleIndex   = -1;

        UpdateScoreUI();
        UpdateTimerUI();

        if (winPanel != null)      winPanel.SetActive(false);
        if (timeOverPanel != null) timeOverPanel.SetActive(false);

        HideAllPortals();
        StartBGM();
        StartSpawnLoop();
    }

    void StartSpawnLoop()
    {
        if (spawnLoopCoroutine != null) StopCoroutine(spawnLoopCoroutine);
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    void StopSpawnLoop()
    {
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        // Let Vuforia fully settle
        yield return new WaitForSeconds(1.0f);

        HideAllPortals();
        yield return new WaitForSeconds(0.2f);

        while (gameRunning)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            if (!moleActive)
                SpawnOneMole();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnOneMole()
    {
        if (portals == null || portals.Length == 0 ||
            molePrefabs == null || molePrefabs.Length == 0) return;

        // Hide any unoccupied portals before spawning
        foreach (Portal p in portals)
            if (p != null && !p.isOccupied)
                p.HidePortal();

        int attempts = 30;
        while (attempts-- > 0)
        {
            int portalIndex = PickRandom(portals.Length, lastPortalIndex);
            int moleIndex   = PickRandom(molePrefabs.Length, lastMoleIndex);

            Portal selected = portals[portalIndex];
            if (selected == null || selected.isOccupied) continue;

            GameObject prefab  = molePrefabs[moleIndex];
            GameObject moleObj = Instantiate(prefab, selected.spawnPoint);

            // FIX: Force activate immediately after spawn.
            // If the prefab was saved as inactive in the Project window,
            // Instantiate produces an inactive clone — Start() never runs,
            // PopRoutine never starts, and the mole stays invisible forever.
            moleObj.SetActive(true);

            moleObj.transform.localPosition = Vector3.zero;
            moleObj.transform.localRotation = Quaternion.identity;

            Mole moleScript = moleObj.GetComponent<Mole>();
            if (moleScript == null)
            {
                Debug.LogError("Prefab missing Mole script: " + prefab.name);
                Destroy(moleObj);
                return;
            }

            moleScript.Setup(selected, this);
            selected.SetMole(moleObj);
            moleActive = true;

            lastPortalIndex = portalIndex;
            lastMoleIndex   = moleIndex;
            return;
        }

        Debug.LogWarning("No free portal found.");
    }

    int PickRandom(int count, int lastIndex)
    {
        if (count == 1) return 0;
        int idx;
        do { idx = Random.Range(0, count); } while (idx == lastIndex);
        return idx;
    }

    // ─────────────────────────────────────────────────────────────
    // Score
    // ─────────────────────────────────────────────────────────────

    public void AddScore(int points)
    {
        if (!gameRunning) return;
        score += points;
        if (score < 0) score = 0;
        UpdateScoreUI();
        if (score >= targetScore) WinGame();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score + " / " + targetScore;
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text  = "Time: " + Mathf.CeilToInt(currentTime);
            timerText.color = currentTime <= 10f ? Color.red : Color.white;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Mole Callback
    // ─────────────────────────────────────────────────────────────

    public void NotifyMoleGone()
    {
        moleActive = false;
    }

    // ─────────────────────────────────────────────────────────────
    // Win / Lose
    // ─────────────────────────────────────────────────────────────

    void WinGame()
    {
        // FIX: NotifyGameOver FIRST — this immediately stops all ARGameActivator
        // coroutines and sets gameOver=true before any other state changes.
        // If called after StopBGM/winPanel, a Vuforia event or in-flight
        // DelayedResume coroutine can still show the tracking lost panel
        // on top of the win screen in the same frame.
        if (arGameActivator != null) arGameActivator.NotifyGameOver();

        gameRunning = false;
        moleActive  = false;
        StopSpawnLoop();
        StopBGM();

        if (winPanel != null) winPanel.SetActive(true);
    }

    void TimeOver()
    {
        // FIX: NotifyGameOver FIRST — same reason as WinGame above
        if (arGameActivator != null) arGameActivator.NotifyGameOver();

        gameRunning  = false;
        moleActive   = false;
        isRestarting = true;
        StopSpawnLoop();
        StopBGM();

        foreach (Portal portal in portals)
        {
            if (portal == null) continue;
            if (portal.currentMole != null) Destroy(portal.currentMole);
            portal.ClearMole();
        }

        if (timeOverPanel != null) timeOverPanel.SetActive(true);
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (timeOverPanel != null) timeOverPanel.SetActive(false);
        if (arGameActivator != null) arGameActivator.ResetStarted();

        score           = 0;
        currentTime     = gameDuration;
        lastPortalIndex = -1;
        lastMoleIndex   = -1;
        moleActive      = false;
        isPaused        = false;
        isRestarting    = false;

        UpdateScoreUI();
        UpdateTimerUI();
        HideAllPortals();

        gameRunning = true;
        StartBGM();
        StartSpawnLoop();
    }

    // ─────────────────────────────────────────────────────────────
    // SFX
    // ─────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}