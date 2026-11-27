using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GuardPatrol))]
[RequireComponent(typeof(GuardAI))]
[RequireComponent(typeof(GuardVision))]
public class GuardChase : MonoBehaviour, IResettable
{
    private enum State
    {
        Patrol,
        Chase,
        Search,
        Look
    }

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float catchDistance = 0.6f;

    [Header("Search Settings")]
    [SerializeField] private float searchSpeed = 2.5f;
    [SerializeField] private float lookTime = 2f;

    private Rigidbody2D rb;

    private GuardPatrol guardPatrol;
    private GuardAI guardAI;
    private GuardVision guardVision;

    private State state = State.Patrol;

    private Vector2 searchTarget;
    private float lookTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        guardPatrol = GetComponent<GuardPatrol>();
        guardAI = GetComponent<GuardAI>();
        guardVision = GetComponent<GuardVision>();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        if (guardVision.IsPlayerDetected)
        {
            if (state != State.Chase)
            {
                state = State.Chase;
                guardAI.SetPatrolEnabled(false);
            }

            ChasePlayer();
            return;
        }

        if (state == State.Chase)
        {
            state = State.Search;
            searchTarget = guardVision.LastSeenPosition;

            guardPatrol.SetNavigationSpeed(searchSpeed);
            guardPatrol.SetNavigationDestination(searchTarget);
        }

        if (state == State.Search)
        {
            if (guardPatrol.HasReachedDestination())
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
                guardAI.SetPatrolEnabled(true);
            }

            return;
        }
    }

    private void ChasePlayer()
    {
        Transform player = guardVision.PlayerTarget;

        if (player == null)
            return;

        if (Vector2.Distance(rb.position, player.position)
            <= catchDistance)
        {
            PlayerStatus status = player.GetComponent<PlayerStatus>();

            if (status != null)
                status.Caught();

            return;
        }

        guardPatrol.SetNavigationSpeed(chaseSpeed);
        guardPatrol.SetNavigationDestination(player.position);
    }

    public void ResetState()
    {
        state = State.Patrol;
        searchTarget = Vector2.zero;
        lookTimer = 0f;

        guardAI.SetPatrolEnabled(true);

        Debug.Log("GuardChase: стан успішно скинуто.", this);
    }
}
