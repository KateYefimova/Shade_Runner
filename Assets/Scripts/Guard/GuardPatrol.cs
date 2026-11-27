using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Seeker))]
public class GuardPatrol : MonoBehaviour, IResettable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.25f;

    private Rigidbody2D rb;
    private AIPath aiPath;
    private Vector2 initialPosition;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiPath = GetComponent<AIPath>();

        initialPosition = rb.position;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (rb.bodyType == RigidbodyType2D.Static)
            rb.bodyType = RigidbodyType2D.Kinematic;

        aiPath.maxSpeed = moveSpeed;
        aiPath.endReachedDistance = arrivalDistance;
        aiPath.canMove = true;
        aiPath.isStopped = true;
    }

    private void Update()
    {
        UpdateFacingDirection();
    }

    public void SetNavigationSpeed(float speed)
    {
        aiPath.maxSpeed = speed;
    }

    public void SetNavigationDestination(Vector2 target)
    {
        aiPath.destination = target;
        aiPath.isStopped = false;
    }

    public void StopNavigation()
    {
        aiPath.isStopped = true;
    }

    public bool HasReachedDestination()
    {
        return !aiPath.pathPending && aiPath.reachedDestination;
    }

    public bool IsPathPending()
    {
        return aiPath.pathPending;
    }

    public Vector2 GetVelocity()
    {
        return aiPath.velocity;
    }

    private void UpdateFacingDirection()
    {
        Vector2 velocity = aiPath.velocity;

        if (velocity.sqrMagnitude > 0.01f)
            FacingDirection = velocity.normalized;
    }

    public void ResetState()
    {
        StopNavigation();

        aiPath.Teleport(initialPosition, true);

        aiPath.maxSpeed = moveSpeed;
        FacingDirection = Vector2.right;

        Debug.Log("GuardPatrol: стан успішно скинуто.", this);
    }
}
