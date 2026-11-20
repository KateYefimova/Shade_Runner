
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class GuardAI : MonoBehaviour
{
    [Header("Patrol Zone")]
    [SerializeField] private BoxCollider2D patrolZone;

    [Header("Patrol Settings")]
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float arrivalDistance = 0.25f;
    [SerializeField] private int maxAttempts = 30;

    private AIPath aiPath;
    private float waitTimer;
    private bool isWaiting;
    private bool hasDestination;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        aiPath = GetComponent<AIPath>();
    }

    private void Start()
    {
        if (patrolZone == null)
        {
            Debug.LogError("GuardAI: Patrol Zone не призначено!", this);
            enabled = false;
            return;
        }

        if (AstarPath.active == null)
        {
            Debug.LogError("GuardAI: AstarPath не знайдено у сцені!", this);
            enabled = false;
            return;
        }

        aiPath.canMove = true;
        ChooseNextDestination();
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ChooseNextDestination();
            }

            return;
        }

        if (!hasDestination)
            return;

        if (aiPath.reachedDestination ||
            Vector2.Distance(transform.position, aiPath.destination)
            <= arrivalDistance)
        {
            isWaiting = true;
            waitTimer = waitTime;
            hasDestination = false;
        }
    }

    private void UpdateFacingDirection()
    {
        Vector2 velocity = aiPath.velocity;

        if (velocity.sqrMagnitude > 0.01f)
            FacingDirection = velocity.normalized;
    }

    private void ChooseNextDestination()
    {
        Bounds bounds = patrolZone.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (!patrolZone.OverlapPoint(candidate))
                continue;

            NNInfo nearest = AstarPath.active.GetNearest(candidate);

            if (nearest.node == null || !nearest.node.Walkable)
                continue;

            Vector3 destination = (Vector3)nearest.position;

            aiPath.destination = destination;
            hasDestination = true;

            Debug.Log("GuardAI: нову ціль обрано: " + destination, this);
            return;
        }

        Debug.LogWarning(
            "GuardAI: не вдалося знайти доступну точку патрулювання. " +
            "Перевірте розмір зони та Grid Graph.",
            this
        );

        hasDestination = false;
    }
}
