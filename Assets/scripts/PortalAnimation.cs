using UnityEngine;

public class PortalAnimation : MonoBehaviour
{
    public float rotateSpeed = 60f;
    public float pulseSpeed  = 3f;
    public float pulseAmount = 0.08f;

    private Vector3 startScale;

    void OnEnable()
    {
        startScale = transform.localScale;
    }

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * pulse;
    }
}