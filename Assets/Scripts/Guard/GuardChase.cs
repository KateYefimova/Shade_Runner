using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GuardPatrol))]
[RequireComponent(typeof(GuardVision))]
public class GuardChase : MonoBehaviour
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
        Vector2 pos = rb.position;
        Vector2 toTarget = target - pos;
        float remaining = toTarget.magnitude;

        if (remaining <= 0.05f)
            return true;

        Vector2 dir = toTarget / remaining;
        guardPatrol.SetFacingDirection(dir);

        float step = Mathf.Min(speed * Time.fixedDeltaTime, remaining);
        float check = step + 0.05f;

        Vector2 move = dir;

        if (guardPatrol.IsPathBlocked(dir, check, out _))
        {
            Vector2 slideX = new Vector2(Mathf.Sign(dir.x), 0f);
            Vector2 slideY = new Vector2(0f, Mathf.Sign(dir.y));

            bool canX = Mathf.Abs(dir.x) > 0.1f &&
                        !guardPatrol.IsPathBlocked(slideX, check, out _);
            bool canY = Mathf.Abs(dir.y) > 0.1f &&
                        !guardPatrol.IsPathBlocked(slideY, check, out _);

            if (canX && (!canY || Mathf.Abs(dir.x) >= Mathf.Abs(dir.y)))
                move = slideX;
            else if (canY)
                move = slideY;
            else
                return true;   
        }

        rb.MovePosition(pos + move * step);
        return false;
    }
}