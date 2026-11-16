using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [Tooltip("Необов'язково: перетягніть сюди PatrolRoute, і точки візьмуться з його дітей")]
    [SerializeField] private Transform routeParent;
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.1f;
    [SerializeField] private float waitTime = 1f;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckDistance = 0.3f;
    [Tooltip("Якщо охоронець застряг на стільки секунд - він іде до наступної точки")]
    [SerializeField] private float stuckSkipTime = 1.5f;

    private Rigidbody2D rb;
    private Collider2D guardCollider;
    private ContactFilter2D obstacleFilter;
    private readonly RaycastHit2D[] castBuffer = new RaycastHit2D[8];

    private bool patrolEnabled = true;
    private int currentWaypointIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float stuckTimer = 0f;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    public void SetPatrolEnabled(bool enabled)
    {
        patrolEnabled = enabled;
    }

    public void SetFacingDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
            FacingDirection = dir.normalized;
    }

    private bool HasRoute => waypoints != null && waypoints.Length >= 2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        guardCollider = GetComponentInChildren<Collider2D>();

        if (rb.bodyType == RigidbodyType2D.Static)
        {
            Debug.LogWarning("Guard: Rigidbody2D був Static, змінено на Kinematic.", this);
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstacleLayer);
        obstacleFilter.useTriggers = false;

        if ((waypoints == null || waypoints.Length == 0) && routeParent != null)
        {
            waypoints = new Transform[routeParent.childCount];
            for (int i = 0; i < waypoints.Length; i++)
                waypoints[i] = routeParent.GetChild(i);
        }

        if (!HasRoute)
        {
            Debug.LogWarning(
                "Guard: потрібно мінімум 2 точки патруля! Заповніть Waypoints " +
                "або перетягнути PatrolRoute в Route Parent.", this);
        }
    }

    private void Start()
    {
        if (HasRoute)
            ResumeFromNearestWaypoint();
    }

    public void ResumeFromNearestWaypoint()
    {
        if (!HasRoute) return;

        float best = float.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float d = Vector2.Distance(rb.position, waypoints[i].position);
            if (d < best)
            {
                best = d;
                currentWaypointIndex = i;
            }
        }

        isWaiting = false;
        stuckTimer = 0f;

        if (best <= arrivalDistance)
            AdvanceIndex();

        Transform t = waypoints[currentWaypointIndex];
        if (t != null)
            SetFacingDirection((Vector2)t.position - rb.position);
    }

    public bool IsPathBlocked(Vector2 dir, float distance, out Collider2D blocker)
    {
        Vector2 size = guardCollider != null
            ? (Vector2)guardCollider.bounds.size * 0.85f
            : Vector2.one * 0.5f;

        int count = Physics2D.BoxCast(
            rb.position, size, rb.rotation, dir,
            obstacleFilter, castBuffer, distance);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = castBuffer[i].collider;

            if (col == null) continue;
            if (col.attachedRigidbody == rb) continue;   

            blocker = col;
            return true;
        }

        blocker = null;
        return false;
    }

    private void FixedUpdate()
    {
        if (!patrolEnabled || !HasRoute)
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
        {
            AdvanceIndex();
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = target.position;

        if (Vector2.Distance(currentPosition, targetPosition) <= arrivalDistance)
        {
            StartWaiting();
            return;
        }

        Vector2 moveDirection = (targetPosition - currentPosition).normalized;
        FacingDirection = moveDirection;

        if (IsPathBlocked(moveDirection, obstacleCheckDistance, out Collider2D blocker))
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckSkipTime)
            {
                Debug.Log("Guard застряг на: " + blocker.name +
                          " -> пропускаю точку " + target.name);
                stuckTimer = 0f;
                AdvanceIndex();
            }
            return;
        }

        stuckTimer = 0f;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition) <= arrivalDistance)
            StartWaiting();
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTime;
        AdvanceIndex();
    }

    private void AdvanceIndex()
    {
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
        Transform[] pts = waypoints;

        if ((pts == null || pts.Length == 0) && routeParent != null)
        {
            pts = new Transform[routeParent.childCount];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = routeParent.GetChild(i);
        }

        if (pts == null || pts.Length < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < pts.Length; i++)
        {
            if (pts[i] == null) continue;

            Gizmos.DrawSphere(pts[i].position, 0.12f);

            if (i < pts.Length - 1 && pts[i + 1] != null)
                Gizmos.DrawLine(pts[i].position, pts[i + 1].position);
        }
    }
}