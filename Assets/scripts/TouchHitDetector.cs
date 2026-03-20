using UnityEngine;

public class TouchHitDetector : MonoBehaviour
{
    public GameManager gameManager;
    private Camera arCamera;

    void Start()
    {
        arCamera = Camera.main;

        if (arCamera == null)
            Debug.LogError("No camera found. Make sure ARCamera is tagged as MainCamera.");
    }

    void Update()
    {
        if (arCamera == null) return;
        if (gameManager != null && gameManager.IsPaused()) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Mole mole = hit.collider.GetComponentInParent<Mole>();
                if (mole != null) mole.Hit();
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Mole mole = hit.collider.GetComponentInParent<Mole>();
                if (mole != null) mole.Hit();
            }
        }
#endif
    }
}