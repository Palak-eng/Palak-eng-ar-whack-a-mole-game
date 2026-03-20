using UnityEngine;
using Vuforia;
using System.Collections;

public class ARGameActivator : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject scanText;
    public GameObject trackingLostPanel;
    public GameManager gameManager;

    private ObserverBehaviour observerBehaviour;
    private bool started  = false;
    private bool gameOver = false;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged += OnStatusChanged;

        ShowScanText(true);
        ShowTrackingLostPanel(false);
    }

    void Update()
    {
        // FIX: Brute-force enforcement every frame after game ends.
        // Vuforia can fire OnTargetStatusChanged on the same frame or
        // next frame as gameOver = true due to Unity's event execution
        // order — no amount of flag-checking in the callback alone can
        // prevent this race. Forcing the panels off in Update() every
        // frame while gameOver is true is the only 100% reliable solution.
        if (gameOver)
        {
            ShowScanText(false);
            ShowTrackingLostPanel(false);
        }
    }

    void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (gameOver) return;

        bool tracked = status.Status == Status.TRACKED;

        if (tracked)
        {
            ShowScanText(false);
            ShowTrackingLostPanel(false);

            if (!started)
            {
                started = true;
                StartCoroutine(DelayedStart());
            }
            else
            {
                StopCoroutine("DelayedResume");
                StartCoroutine(DelayedResume());
            }
        }
        else
        {
            StopCoroutine("DelayedResume");

            if (!started)
            {
                ShowScanText(true);
                ShowTrackingLostPanel(false);
            }
            else
            {
                ShowScanText(false);
                ShowTrackingLostPanel(true);
                gameManager.PauseGame();
            }
        }
    }

    public void NotifyGameOver()
    {
        gameOver = true;
        StopAllCoroutines();
        ShowScanText(false);
        ShowTrackingLostPanel(false);
    }

    public void ResetStarted()
    {
        started  = false;
        gameOver = false;
        ShowScanText(true);
        ShowTrackingLostPanel(false);
    }

    void ShowScanText(bool show)
    {
        if (scanText != null) scanText.SetActive(show);
    }

    void ShowTrackingLostPanel(bool show)
    {
        if (trackingLostPanel != null) trackingLostPanel.SetActive(show);
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.3f);

        if (gameOver) yield break;

        gameManager.HideAllPortals();
        yield return new WaitForSeconds(0.1f);

        if (gameOver) yield break;

        gameManager.StartGame();
    }

    IEnumerator DelayedResume()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (gameOver) yield break;

        gameManager.HideAllPortals();
        gameManager.ResumeGame();
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged -= OnStatusChanged;
    }
}