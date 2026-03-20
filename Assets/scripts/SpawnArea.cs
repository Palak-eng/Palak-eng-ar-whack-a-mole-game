using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    private BoxCollider boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
            Debug.LogError("SpawnArea needs a BoxCollider.");
    }

    public Vector3 GetRandomPointInside()
    {
        if (boxCollider == null)
            return transform.position;

        Vector3 center = boxCollider.center;
        Vector3 size   = boxCollider.size;

        float randomX = Random.Range(-size.x / 2f, size.x / 2f);
        float randomY = Random.Range(-size.y / 2f, size.y / 2f);
        float randomZ = Random.Range(-size.z / 2f, size.z / 2f);

        return transform.TransformPoint(center + new Vector3(randomX, randomY, randomZ));
    }
}