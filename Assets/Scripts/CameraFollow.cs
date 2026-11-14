using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Об'єкт гравця, за яким стежимо
    public float smoothSpeed = 5f; // Швидкість плавного слідування
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Зміщення (Z обов'язково -10 для 2D)

    [Header("Межі кімнати (Bounds)")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        // Цільова позиція з урахуванням зміщення
        Vector3 targetPosition = target.position + offset;

        // Обмежуємо координати камери в межах рівня
        float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Збираємо кінцеву позицію (зберігаючи Z-координату камери)
        Vector3 boundedPosition = new Vector3(clampedX, clampedY, offset.z);

        // Плавне переміщення камери (Lerp)
        transform.position = Vector3.Lerp(transform.position, boundedPosition, smoothSpeed * Time.deltaTime);
    }
}