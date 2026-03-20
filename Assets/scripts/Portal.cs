using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    public Transform spawnPoint;
    public bool isOccupied = false;
    public GameObject currentMole;

    // Assign the visual child object of the portal in the Inspector
    public GameObject portalVisual;

    private PortalAnimation portalAnimation;

    void Awake()
    {
        portalAnimation = GetComponentInChildren<PortalAnimation>(true);
        HidePortal();
    }

    void Start()
    {
        StartCoroutine(DelayedHide());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);

        if (!isOccupied)
            HidePortal();
    }

    public void SetMole(GameObject mole)
    {
        currentMole = mole;
        isOccupied  = true;
        ShowPortal();
    }

    public void ClearMole()
    {
        currentMole = null;
        isOccupied  = false;
        HidePortal();
    }

    public void ShowPortal()
    {
        if (portalVisual != null) portalVisual.SetActive(true);
        if (portalAnimation != null) portalAnimation.enabled = true;
    }

    public void HidePortal()
    {
        if (portalVisual != null) portalVisual.SetActive(false);
        if (portalAnimation != null) portalAnimation.enabled = false;
    }
}