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

    [Header("Repath")]
    [Tooltip("Мінімальна зміна цілі (в юнітах), після якої запитуємо новий шлях.")]
    [SerializeField] private float minDestinationChange = 0.4f;
    [Tooltip("Мінімальний інтервал між запитами шляху (сек).")]
    [SerializeField] private float minRepathInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    
    [Header("Steering")]
    [Tooltip("На скільки юнітів уперед по шляху цілиться AIPath. Менше = менше зрізає кути.")]
    [SerializeField] private float lookAheadDistance = 0.5f;

    private Rigidbody2D rb;
    private AIPath aiPath;
    private Vector2 initialPosition;

    private Vector2 lastRequestedTarget;
    private bool hasRequestedTarget;
    private float lastRepathTime = -999f;

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
        aiPath.pickNextWaypointDist = lookAheadDistance;
        aiPath.canMove = true;
        aiPath.isStopped = true;

        // AIPath не повинен крутити transform (разом з ним крутився б колайдер)
        aiPath.enableRotation = false;
        
        aiPath.slowWhenNotFacingTarget = false;
        // У 2D-грі нам не потрібна гравітація від AIPath
        aiPath.gravity = Vector3.zero;
    }

    private void Update()
    {
        UpdateFacingDirection();
    }

    public void SetNavigationSpeed(float speed)
    {
        aiPath.maxSpeed = speed;
    }

    // Центр найближчого прохідного вузла гарантовано має запас до стін
    private Vector2 SnapToNodeCenter(Vector2 target)
    {
        if (AstarPath.active == null)
            return target;

        NNInfo nearest = AstarPath.active.GetNearest(target, NNConstraint.Default);

        if (nearest.node == null)
            return target;

        return (Vector3)nearest.node.position;
    }

    public void SetNavigationDestination(Vector2 target)
    {
        target = SnapToNodeCenter(target);

        bool wasStopped = aiPath.isStopped;

        bool targetMoved =
            !hasRequestedTarget ||
            (target - lastRequestedTarget).sqrMagnitude >=
            minDestinationChange * minDestinationChange;

        // Уже йдемо до (майже) цієї цілі: нічого не робимо
        if (!wasStopped && !targetMoved)
            return;

        // Не спамимо запитами шляху (важливо для погоні за гравцем)
        if (!wasStopped && Time.time - lastRepathTime < minRepathInterval)
            return;

        lastRequestedTarget = target;
        hasRequestedTarget = true;
        lastRepathTime = Time.time;

        aiPath.destination = target;
        aiPath.isStopped = false;
        aiPath.SearchPath();
    }

    public void StopNavigation()
    {
        aiPath.isStopped = true;
        hasRequestedTarget = false; // наступна ціль завжди перерахує шлях
    }

    public bool HasReachedDestination()
    {
        if (aiPath.isStopped || aiPath.pathPending || !aiPath.hasPath)
            return false;

        if (aiPath.reachedDestination)
        {
            if (debugLogs)
                Debug.Log("GuardPatrol: дійшов до цілі.", this);
            return true;
        }

        // Частковий шлях (ціль недосяжна): дійшли до кінця того, що є
        if (aiPath.reachedEndOfPath)
        {
            if (debugLogs)
                Debug.LogWarning(
                    "GuardPatrol: кінець ЧАСТКОВОГО шляху (ціль недосяжна).",
                    this);
            return true;
        }

        return false;
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