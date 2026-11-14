using UnityEngine;

[RequireComponent(typeof(GuardPatrol))]
public class GuardVision : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private Transform playerTarget;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private float viewAngle = 90f;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleLayer;

    private GuardPatrol guardPatrol;
    private bool playerDetected = false;

    private void Awake()
    {
        guardPatrol = GetComponent<GuardPatrol>();
    }

    private void Update()
    {
        if (playerTarget == null)
            return;

        Vector2 directionToPlayer =
            (playerTarget.position - transform.position).normalized;

        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerTarget.position
        );

        // Check distance
        if (distanceToPlayer > viewDistance)
        {
            SetDetection(false);
            return;
        }

        // Check field of view angle
        float angle = Vector2.Angle(
            guardPatrol.FacingDirection,
            directionToPlayer
        );

        if (angle > viewAngle / 2f)
        {
            SetDetection(false);
            return;
        }

        // Check if an obstacle blocks the vision
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            SetDetection(false);
            return;
        }

        SetDetection(true);
    }

    private void SetDetection(bool detected)
    {
        if (playerDetected == detected)
            return;

        playerDetected = detected;

        if (playerDetected)
        {
            Debug.Log("Guard detected the player!");
        }
        else
        {
            Debug.Log("Guard lost sight of the player.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        GuardPatrol patrol = GetComponent<GuardPatrol>();

        if (patrol == null)
            return;

        Vector2 facing = patrol.FacingDirection;

        if (facing.sqrMagnitude < 0.001f)
            return;

        float halfAngle = viewAngle / 2f;

        Vector2 leftDirection =
            Quaternion.Euler(0, 0, halfAngle) * facing;

        Vector2 rightDirection =
            Quaternion.Euler(0, 0, -halfAngle) * facing;

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            transform.position,
            leftDirection * viewDistance
        );

        Gizmos.DrawRay(
            transform.position,
            rightDirection * viewDistance
        );

        Gizmos.DrawRay(
            transform.position,
            facing * viewDistance
        );
    }
}