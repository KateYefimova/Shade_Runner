using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GuardPatrol))]
[RequireComponent(typeof(GuardVision))]
public class GuardChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;

    private Rigidbody2D rb;
    private GuardPatrol guardPatrol;
    private GuardVision guardVision;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        guardPatrol = GetComponent<GuardPatrol>();
        guardVision = GetComponent<GuardVision>();
    }

    private void FixedUpdate()
    {
        if (guardVision.IsPlayerDetected)
        {
            ChasePlayer();
        }
        else
        {
            // Return to normal patrol
            guardPatrol.SetPatrolEnabled(true);
        }
    }

    private void ChasePlayer()
    {
        Transform player = guardVision.PlayerTarget;

        if (player == null)
            return;

        // Disable patrol while chasing
        guardPatrol.SetPatrolEnabled(false);

        Vector2 direction = (
            (Vector2)player.position - rb.position
        ).normalized;

        // Update guard facing direction
        guardPatrol.SetFacingDirection(direction);

        // Move directly toward the player
        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            player.position,
            chaseSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }
}