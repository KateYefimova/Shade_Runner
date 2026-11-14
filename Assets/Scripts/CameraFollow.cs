using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; 
    public float smoothSpeed = 5f; // Швидкість плавного слідування
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Зміщення 

    [Header("Межі кімнати (Bounds)")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

        Vector3 boundedPosition = new Vector3(clampedX, clampedY, offset.z);

        transform.position = Vector3.Lerp(transform.position, boundedPosition, smoothSpeed * Time.deltaTime);
    }
}