using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(GuardPatrol))]
public class GuardAI : MonoBehaviour, IResettable
{
    [Header("Patrol Route")]
    [Tooltip("Маршрут цього охоронця (об'єкт PatrolRoute з точками-дітьми).")]
    [SerializeField] private PatrolRoute route;
    [Tooltip("З якої точки починає (0 = перша). Розведи охоронців по різних точках.")]
    [SerializeField] private int startIndex = 0;

    [Header("Patrol Settings")]
    [SerializeField] private float waitTime = 2f;
    [Tooltip("Якщо точка маршруту далі за це значення від прохідного вузла графа, у консолі буде попередження.")]
    [SerializeField] private float maxPointDistanceToGraph = 0.4f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private GuardPatrol guardPatrol;

    private bool patrolEnabled = true;
    private bool isWaiting;
    private bool hasDestination;
    private bool needsDestination = true;

    private float waitTimer;
    private int currentIndex;
    private int direction = 1;

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
        if (route == null || route.Count == 0)
        {
            Debug.LogError("GuardAI: Patrol Route не призначено або в ньому немає точок!", this);
            enabled = false;
            return;
        }

        if (AstarPath.active == null)
        {
            Debug.LogError("GuardAI: AstarPath не знайдено у сцені!", this);
            enabled = false;
            return;
        }

        ValidateRoute();
        ResetRouteProgress();
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
                currentIndex = route.GetNextIndex(currentIndex, ref direction);
                needsDestination = true;
            }

            return;
        }

        // Рух запускаємо лише тут (а не в ResetState / SetPatrolEnabled),
        // щоб порядок викликів ResetState у різних скриптах не мав значення.
        if (needsDestination)
        {
            GoToCurrentPoint();
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

    // false: патруль призупинено (погоня/пошук). true: продовжуємо маршрут
    // з тієї самої точки, до якої охоронець ішов або в якій стояв.
    public void SetPatrolEnabled(bool value)
    {
        if (patrolEnabled == value)
            return;

        patrolEnabled = value;

        if (!value)
        {
            isWaiting = false;
            hasDestination = false;
            needsDestination = false;
            guardPatrol.StopNavigation();
            return;
        }

        isWaiting = false;
        waitTimer = 0f;
        needsDestination = true;
    }

    public void ResetState()
    {
        patrolEnabled = true;
        isWaiting = false;
        hasDestination = false;
        waitTimer = 0f;

        ResetRouteProgress();

        if (debugLogs)
            Debug.Log("GuardAI: стан успішно скинуто.", this);
    }

    private void ResetRouteProgress()
    {
        direction = 1;
        needsDestination = true;

        if (route != null && route.Count > 0)
            currentIndex = Mathf.Clamp(startIndex, 0, route.Count - 1);
    }

    private void GoToCurrentPoint()
    {
        needsDestination = false;
        hasDestination = true;

        if (debugLogs)
            Debug.Log(
                $"GuardAI ({name}): іде до точки {currentIndex + 1}/{route.Count}.",
                this);

        guardPatrol.SetNavigationSpeed(patrolSpeed);
        guardPatrol.SetNavigationDestination(route.GetPosition(currentIndex));
    }

    private void ValidateRoute()
    {
        for (int i = 0; i < route.Count; i++)
        {
            Vector2 point = route.GetPosition(i);
            NNInfo nearest = AstarPath.active.GetNearest(point, NNConstraint.Default);

            if (nearest.node == null)
            {
                Debug.LogWarning(
                    $"GuardAI: точка {i + 1} маршруту '{route.name}' поза графом A*.",
                    route);
                continue;
            }

            Vector2 nodePos = (Vector3)nearest.node.position;
            float dist = Vector2.Distance(point, nodePos);

            if (dist > maxPointDistanceToGraph)
            {
                Debug.LogWarning(
                    $"GuardAI: точка {i + 1} маршруту '{route.name}' далеко ({dist:F2}) " +
                    "від прохідного вузла: схоже, вона в стіні, на ящику або поза графом. Посунь її.",
                    route);
            }
        }
    }
}