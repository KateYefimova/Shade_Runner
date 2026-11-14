
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DoorInteractable : MonoBehaviour
{
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float rotationSpeed = 150f;

    private Rigidbody2D rb;
    private float closedAngle;
    private bool isOpen = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        closedAngle = rb.rotation;
    }

    public void Interact()
    {
        isOpen = !isOpen;

        Debug.Log(isOpen ? "Door opening" : "Door closing");
    }

    private void FixedUpdate()
    {
        float targetAngle = isOpen
            ? closedAngle + openAngle
            : closedAngle;

        float newAngle = Mathf.MoveTowardsAngle(
            rb.rotation,
            targetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }
}
