using UnityEngine;
using System.Collections;

public class Mole : MonoBehaviour
{
    [Header("Movement")]
    public float popHeight   = 0.08f;
    public Vector3 popDirection  = Vector3.up; // Set in Inspector: try (0,1,0) for +Y, (0,0,1) for +Z
    public float moveSpeed   = 3f;
    public float visibleTime = 2.0f;

    [Header("Scoring")]
    public int  points = 1;
    public bool isBad  = false;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public AudioClip  hitSound;

    [Header("Coin Effect")]
    public GameObject floatingTextPrefab;
    public float coinPopHeight = 0.03f;
    public float coinMaxScale  = 0.05f;
    public float coinDuration  = 0.5f;
    public float coinLifetime  = 1f;

    private Vector3 startWorldPos;
    private Vector3 topWorldPos;
    private Vector3 startScale;
    private bool    isHit = false;
    private Portal      parentPortal;
    private GameManager gameManager;

    public void Setup(Portal portal, GameManager manager)
    {
        parentPortal = portal;
        gameManager  = manager;
    }

    void Start()
    {
        startScale    = transform.localScale;
        startWorldPos = transform.position;
        topWorldPos   = startWorldPos + popDirection.normalized * popHeight;

        StartCoroutine(PopRoutine());
    }

    IEnumerator PopRoutine()
    {
        float riseDuration = Mathf.Max(popHeight / moveSpeed, 0.2f);
        float t = 0f;

        // ── Phase 1: Anticipation squash (tiny pre-squash before rising) ──
        // Squash down briefly to "wind up" before popping out
        float anticipationTime = 0.08f;
        float at = 0f;
        while (at < anticipationTime)
        {
            at += Time.deltaTime;
            float p = at / anticipationTime;
            transform.localScale = new Vector3(
                startScale.x * Mathf.Lerp(1f, 1.3f, p),   // widen
                startScale.y * Mathf.Lerp(1f, 0.7f, p),   // squash down
                startScale.z * Mathf.Lerp(1f, 1.3f, p));
            yield return null;
        }

        // ── Phase 2: Fast pop up with stretch ──
        while (t < riseDuration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / riseDuration);

            // Ease out cubic — fast start, smooth landing at top
            float eased = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = Vector3.Lerp(startWorldPos, topWorldPos, eased);

            // Stretch tall while rising, settle to normal at top
            float stretchY = Mathf.Lerp(1.4f, 1f, eased);
            float squishXZ = Mathf.Lerp(0.7f, 1f, eased);
            transform.localScale = new Vector3(
                startScale.x * squishXZ,
                startScale.y * stretchY,
                startScale.z * squishXZ);

            yield return null;
        }

        // ── Phase 3: Overshoot bounce at the top ──
        // Goes slightly past topPos then settles back — gives a bouncy feel
        float bounceTime = 0.12f;
        float bt = 0f;
        Vector3 overshootPos = topWorldPos + popDirection.normalized * (popHeight * 0.15f);

        while (bt < bounceTime)
        {
            bt += Time.deltaTime;
            float p = bt / bounceTime;
            // Ping-pong: goes to overshoot then back to top
            float pingpong = Mathf.Sin(p * Mathf.PI);
            transform.position = Vector3.Lerp(topWorldPos, overshootPos, pingpong * 0.5f);

            // Squash slightly at peak of overshoot
            float squish = 1f + Mathf.Sin(p * Mathf.PI) * 0.08f;
            transform.localScale = new Vector3(
                startScale.x * (1f + squish * 0.05f),
                startScale.y * (1f - squish * 0.05f),
                startScale.z * (1f + squish * 0.05f));
            yield return null;
        }

        // Snap to exact top position and reset scale cleanly
        transform.position   = topWorldPos;
        transform.localScale = startScale;

        // ── Phase 4: Idle wobble while visible ──
        // Gentle side-to-side scale pulse so the mole feels "alive"
        float elapsed = 0f;
        while (elapsed < visibleTime)
        {
            elapsed += Time.deltaTime;
            float wobble = 1f + Mathf.Sin(elapsed * 8f) * 0.03f;
            transform.localScale = new Vector3(
                startScale.x * wobble,
                startScale.y * (2f - wobble),   // opposite axis for organic feel
                startScale.z * wobble);
            yield return null;
        }

        transform.localScale = startScale;

        if (!isHit)
            yield return StartCoroutine(GoDownAndDestroy());
    }

    IEnumerator GoDownAndDestroy()
    {
        // Quick squash before going down (anticipation)
        float squashTime = 0.07f;
        float st = 0f;
        while (st < squashTime)
        {
            st += Time.deltaTime;
            float p = st / squashTime;
            transform.localScale = new Vector3(
                startScale.x * Mathf.Lerp(1f, 1.2f, p),
                startScale.y * Mathf.Lerp(1f, 0.8f, p),
                startScale.z * Mathf.Lerp(1f, 1.2f, p));
            yield return null;
        }

        // Slide back down
        while (Vector3.Distance(transform.position, startWorldPos) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, startWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (parentPortal != null) parentPortal.ClearMole();
        if (gameManager  != null) gameManager.NotifyMoleGone();
        Destroy(gameObject);
    }

    public void Hit()
    {
        if (isHit) return;
        isHit = true;

        if (gameManager != null) gameManager.AddScore(points);

        if (hitEffectPrefab != null)
        {
            GameObject fx = Instantiate(hitEffectPrefab,
                transform.position + Vector3.up * 0.02f, Quaternion.identity);
            Destroy(fx, 1f);
        }

        if (gameManager != null) gameManager.PlaySFX(hitSound);

        SpawnCoin();
        StartCoroutine(HitAndDestroy());
    }

    void SpawnCoin()
    {
        if (floatingTextPrefab == null) return;

        GameObject coin = Instantiate(floatingTextPrefab,
            transform.position + Vector3.up * 0.05f, Quaternion.identity);
        Destroy(coin, coinLifetime);
        StartCoroutine(CoinPopAndDisappear(coin));
    }

    IEnumerator CoinPopAndDisappear(GameObject coin)
    {
        if (coin == null) yield break;

        float   t        = 0f;
        Vector3 startPos = coin.transform.position;
        Vector3 peakPos  = startPos + Vector3.up * coinPopHeight;

        while (t < coinDuration * 0.5f)
        {
            if (coin == null) yield break;
            t += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - t / (coinDuration * 0.5f), 2f);
            coin.transform.position   = Vector3.Lerp(startPos, peakPos, eased);
            coin.transform.localScale = Vector3.one * Mathf.Lerp(0f, coinMaxScale, eased);
            coin.transform.Rotate(0f, 360f * Time.deltaTime, 0f);
            yield return null;
        }

        t = 0f;
        Vector3 peakScale = coin.transform.localScale;
        while (t < coinDuration * 0.5f)
        {
            if (coin == null) yield break;
            t += Time.deltaTime;
            coin.transform.localScale = Vector3.Lerp(peakScale, Vector3.zero, t / (coinDuration * 0.5f));
            coin.transform.Rotate(0f, 360f * Time.deltaTime, 0f);
            yield return null;
        }

        if (coin != null) Destroy(coin);
    }

    IEnumerator HitAndDestroy()
    {
        float duration = 0.35f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            // Spin + shrink + squash on hit for a satisfying smack feel
            transform.Rotate(0f, 900f * Time.deltaTime, 0f);
            float scaleDown = Mathf.Lerp(1f, 0f, progress * progress); // ease in
            transform.localScale = new Vector3(
                startScale.x * scaleDown * Mathf.Lerp(1.3f, 1f, progress),
                startScale.y * scaleDown * Mathf.Lerp(0.5f, 1f, progress),
                startScale.z * scaleDown * Mathf.Lerp(1.3f, 1f, progress));
            yield return null;
        }

        if (parentPortal != null) parentPortal.ClearMole();
        if (gameManager  != null) gameManager.NotifyMoleGone();
        Destroy(gameObject);
    }
}