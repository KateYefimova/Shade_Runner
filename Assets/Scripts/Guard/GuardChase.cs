using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GuardPatrol))]
[RequireComponent(typeof(GuardVision))]
public class GuardChase : MonoBehaviour, IResettable
{
    private enum State { Patrol, Chase, Search, Look }

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [Tooltip("Відстань, на якій охоронець ловить гравця")]
    [SerializeField] private float catchDistance = 0.6f;

    [Header("After losing the player")]
    [SerializeField] private float searchSpeed = 2.5f;
    [Tooltip("Скільки секунд охоронець роззирається на останньому місці")]
    [SerializeField] private float lookTime = 2f;

    private Rigidbody2D rb;
    private GuardPatrol guardPatrol;
    private GuardVision guardVision;

    private State state = State.Patrol;
    private Vector2 searchTarget;
    private float lookTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        guardPatrol = GetComponent<GuardPatrol>();
        guardVision = GetComponent<GuardVision>();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (guardVision.IsPlayerDetected)
        {
            state = State.Chase;
            guardPatrol.SetPatrolEnabled(false);
            ChasePlayer();
            return;
        }

        if (state == State.Chase)
        {
            state = State.Search;
            searchTarget = guardVision.LastSeenPosition;
        }

        if (state == State.Search)
        {
            if (StepToward(searchTarget, searchSpeed))
            {
                state = State.Look;
                lookTimer = lookTime;
                guardPatrol.StopNavigation();
            }
            return;
        }

        if (state == State.Look)
        {
            lookTimer -= Time.fixedDeltaTime;

            if (lookTimer <= 0f)
            {
                state = State.Patrol;
                guardPatrol.ResumeFromNearestWaypoint();
                guardPatrol.SetPatrolEnabled(true);
            }
            return;
        }

        guardPatrol.SetPatrolEnabled(true);
    }

    private void ChasePlayer()
    {
        Transform player = guardVision.PlayerTarget;
        if (player == null) return;

        if (Vector2.Distance(rb.position, player.position) <= catchDistance)
        {
            PlayerStatus status = player.GetComponent<PlayerStatus>();
            if (status != null)
                status.Caught();
            return;
        }

        StepToward(player.position, chaseSpeed);
    }

    private bool StepToward(Vector2 target, float speed)
    {
        guardPatrol.SetNavigationSpeed(speed);
        guardPatrol.SetNavigationDestination(target);

        return guardPatrol.HasReachedDestination();
    }
    
    public void ResetState()
    {
        state = State.Patrol;
        searchTarget = Vector2.zero;
        lookTimer = 0f;

        Debug.Log("GuardChase: стан успішно скинуто.");
    }
}