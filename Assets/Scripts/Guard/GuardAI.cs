using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(GuardPatrol))]
public class GuardAI : MonoBehaviour, IResettable
{
    [Header("Patrol Zone")]
    [SerializeField] private BoxCollider2D patrolZone;

    [Header("Patrol Settings")]
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private int maxAttempts = 30;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    private GuardPatrol guardPatrol;

    private bool patrolEnabled = true;
    private bool isWaiting;
    private bool hasDestination;

    private float waitTimer;

    public Vector2 FacingDirection =>
        guardPatrol != null
            ? guardPatrol.FacingDirection
            : Vector2.right;

    private void Awake()
    {
        guardPatrol = GetComponent<GuardPatrol>();
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

        guardPatrol.SetNavigationSpeed(patrolSpeed);
        ChooseNextDestination();
    }

    private void Update()
    {
        if (!patrolEnabled)
            return;

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

        if (guardPatrol.HasReachedDestination())
        {
            hasDestination = false;
            isWaiting = true;
            waitTimer = waitTime;
            guardPatrol.StopNavigation();
        }
    }

    public void SetPatrolEnabled(bool value)
    {
        if (patrolEnabled == value)
            return;

        patrolEnabled = value;

        if (!value)
        {
            isWaiting = false;
            hasDestination = false;
            guardPatrol.StopNavigation();
            return;
        }

        isWaiting = false;
        waitTimer = 0f;

        // Після погоні/пошуку повертаємо швидкість патрулювання
        guardPatrol.SetNavigationSpeed(patrolSpeed);

        ChooseNextDestination();
    }

    public void ResetState()
    {
        patrolEnabled = true;
        isWaiting = false;
        hasDestination = false;
        waitTimer = 0f;

        guardPatrol.SetNavigationSpeed(patrolSpeed);

        if (enabled)
            ChooseNextDestination();

        Debug.Log("GuardAI: стан успішно скинуто.", this);
    }
    private void ChooseNextDestination()
    {
        if (!patrolEnabled || patrolZone == null)
            return;

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

            Vector2 graphPosition = (Vector3)nearest.node.position;
            // Не дозволяємо вибрати точку за межами зони патрулювання.
            if (!patrolZone.OverlapPoint(graphPosition))
                continue;

            hasDestination = true;
            guardPatrol.SetNavigationDestination(graphPosition);

            return;
        }

        hasDestination = false;
        guardPatrol.StopNavigation();

        Debug.LogWarning(
            "GuardAI: не вдалося знайти доступну точку патрулювання. " +
            "Перевір Patrol Zone та A* Graph.",
            this
        );
    }
}
