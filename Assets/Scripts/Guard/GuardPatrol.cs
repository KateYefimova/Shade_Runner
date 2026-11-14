
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.1f;
    [SerializeField] private float waitTime = 1f;
    
    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckDistance = 0.3f;

    private Collider2D guardCollider;

    private Rigidbody2D rb;
    public Vector2 FacingDirection { get; private set; }

    private int currentWaypointIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        guardCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
                isWaiting = false;

            return;
        }

        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = target.position;

// Check if Guard has already reached the waypoint
        if (Vector2.Distance(currentPosition, targetPosition) <= arrivalDistance)
        {
            StartWaiting();
            return;
        }

        Vector2 directionToTarget = targetPosition - currentPosition;

        Vector2 moveDirection = directionToTarget.normalized;
        FacingDirection = moveDirection;
        
        Vector2 checkSize = guardCollider != null
            ? guardCollider.bounds.size * 0.85f
            : Vector2.one * 0.5f;

        RaycastHit2D obstacleHit = Physics2D.BoxCast(
            currentPosition,
            checkSize,
            rb.rotation,
            moveDirection,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (obstacleHit.collider != null)
        {
            Debug.Log(
                "Guard stopped! Obstacle: " +
                obstacleHit.collider.name +
                " | Target: " + target.name +
                " | Waypoint index: " + currentWaypointIndex
            );

            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition)
            <= arrivalDistance)
        {
            rb.MovePosition(targetPosition);
            StartWaiting();
        }
    }

    private void StartWaiting()
    {
        Debug.Log("Reached waypoint: " + waypoints[currentWaypointIndex].name);

        isWaiting = true;
        waitTimer = waitTime;

        currentWaypointIndex += direction;

        if (currentWaypointIndex >= waypoints.Length)
        {
            direction = -1;
            currentWaypointIndex = waypoints.Length - 2;
        }
        else if (currentWaypointIndex < 0)
        {
            direction = 1;
            currentWaypointIndex = 1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.12f);

            if (i < waypoints.Length - 1 &&
                waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(
                    waypoints[i].position,
                    waypoints[i + 1].position
                );
            }
        }
    }
}
