
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Seeker))]
public class GuardPatrol : MonoBehaviour, IResettable{
    [Header("Patrol Route")]
    [Tooltip("Необов'язково: перетягніть сюди PatrolRoute")]
    [SerializeField] private Transform routeParent;
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arrivalDistance = 0.2f;
    [SerializeField] private float waitTime = 1f;

    private Rigidbody2D rb;
    private AIPath aiPath;
    private Vector2 initialPosition;

    private bool patrolEnabled = true;
    private int currentWaypointIndex;
    private int direction = 1;

    private bool isWaiting;
    private float waitTimer;

    private Vector2 lastDestination;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private bool HasRoute =>
        waypoints != null && waypoints.Length >= 2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        aiPath = GetComponent<AIPath>();
        initialPosition = rb.position;

        if (rb.bodyType == RigidbodyType2D.Static)
        {
            Debug.LogWarning(
                "Guard: Rigidbody2D був Static, змінено на Kinematic.",
                this
            );

            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if ((waypoints == null || waypoints.Length == 0)
            && routeParent != null)
        {
            waypoints = new Transform[routeParent.childCount];

            for (int i = 0; i < waypoints.Length; i++)
                waypoints[i] = routeParent.GetChild(i);
        }

        aiPath.maxSpeed = moveSpeed;
        aiPath.endReachedDistance = arrivalDistance;
        aiPath.canMove = true;
        aiPath.isStopped = true;

        if (!HasRoute)
        {
            Debug.LogWarning(
                "Guard: потрібно мінімум 2 точки патруля!",
                this
            );
        }
    }

    private void Start()
    {
        if (AstarPath.active == null)
        {
            Debug.LogError(
                "Guard: AstarPath не знайдений у сцені!",
                this
            );

            return;
        }

        if (HasRoute)
            ResumeFromNearestWaypoint();
    }

    private void FixedUpdate()
    {
        UpdateFacingDirection();

        if (!patrolEnabled || !HasRoute)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToCurrentWaypoint();
            }

            return;
        }

        if (!aiPath.pathPending && aiPath.reachedDestination)
        {
            StartWaiting();
        }
    }

    // =========================
    // PATROL
    // =========================

    private void MoveToCurrentWaypoint()
    {
        if (!HasRoute)
            return;

        int checkedPoints = 0;

        while (checkedPoints < waypoints.Length)
        {
            Transform target = waypoints[currentWaypointIndex];

            if (target != null)
            {
                SetNavigationDestination(target.position);
                return;
            }

            AdvanceIndex();
            checkedPoints++;
        }

        Debug.LogWarning(
            "Guard: усі точки маршруту порожні!",
            this
        );

        StopNavigation();
    }

    private void StartWaiting()
    {
        if (isWaiting)
            return;

        isWaiting = true;
        waitTimer = waitTime;

        StopNavigation();
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

    public void ResumeFromNearestWaypoint()
    {
        if (!HasRoute)
            return;

        float bestDistance = float.MaxValue;
        int nearestIndex = -1;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            float distance = Vector2.Distance(
                rb.position,
                waypoints[i].position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex < 0)
            return;

        currentWaypointIndex = nearestIndex;
        direction = 1;

        isWaiting = false;
        waitTimer = 0f;

        MoveToCurrentWaypoint();
    }

    public void SetPatrolEnabled(bool value)
    {
        if (patrolEnabled == value)
            return;

        patrolEnabled = value;

        if (!value)
        {
            isWaiting = false;
            StopNavigation();
        }
        else if (!isWaiting && HasRoute)
        {
            MoveToCurrentWaypoint();
        }
    }

    // =========================
    // A* NAVIGATION API
    // =========================

    public void SetNavigationSpeed(float speed)
    {
        aiPath.maxSpeed = speed;
    }

    public void SetNavigationDestination(Vector2 target)
    {
        if (aiPath == null)
            return;

        bool destinationChanged =
            Vector2.Distance(lastDestination, target) > 0.15f;

        if (destinationChanged)
        {
            lastDestination = target;
            aiPath.destination = target;
        }

        aiPath.isStopped = false;
    }

    public void StopNavigation()
    {
        if (aiPath != null)
            aiPath.isStopped = true;
    }

    public bool HasReachedDestination()
    {
        return !aiPath.pathPending && aiPath.reachedDestination;
    }

    // =========================
    // FACING DIRECTION
    // =========================

    public void SetFacingDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
            FacingDirection = dir.normalized;
    }

    private void UpdateFacingDirection()
    {
        if (aiPath == null)
            return;

        Vector2 velocity = aiPath.velocity;

        if (velocity.sqrMagnitude > 0.01f)
            SetFacingDirection(velocity);
    }

    // =========================
    // DEBUG
    // =========================

    private void OnDrawGizmosSelected()
    {
        Transform[] points = waypoints;

        if ((points == null || points.Length == 0)
            && routeParent != null)
        {
            points = new Transform[routeParent.childCount];

            for (int i = 0; i < points.Length; i++)
                points[i] = routeParent.GetChild(i);
        }

        if (points == null || points.Length < 2)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
                continue;

            Gizmos.DrawSphere(points[i].position, 0.12f);

            if (i < points.Length - 1 && points[i + 1] != null)
            {
                Gizmos.DrawLine(
                    points[i].position,
                    points[i + 1].position
                );
            }
        }
    }
    
    public void ResetState()
    {
        // Зупиняємо поточний рух
        StopNavigation();

        // Повертаємо охоронця на початкову позицію
        aiPath.Teleport(initialPosition, true);

        // Скидаємо стан патрулювання
        patrolEnabled = true;
        currentWaypointIndex = 0;
        direction = 1;

        isWaiting = false;
        waitTimer = 0f;

        lastDestination = Vector2.positiveInfinity;

        // Відновлюємо початковий напрямок
        FacingDirection = Vector2.right;

        // Запускаємо патрулювання з першої точки
        MoveToCurrentWaypoint();

        Debug.Log("GuardPatrol: стан успішно скинуто.");
    }
}
