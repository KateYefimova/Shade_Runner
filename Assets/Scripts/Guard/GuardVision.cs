using UnityEngine;

public class GuardVision : MonoBehaviour, IResettable
{
    [Header("Player Detection")]
    [Tooltip("Якщо порожньо - знайдеться об'єкт з PlayerStatus або тегом Player")]
    [SerializeField] private Transform playerTarget;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private float viewAngle = 90f;

    [Header("Alert")]
    [Tooltip("Скільки секунд у світлі до Game Over")]
    [SerializeField] private float timeToEscape = 5f;
    [Tooltip("Скільки секунд таймера відновлюється за секунду поза світлом")]
    [SerializeField] private float recoverSpeed = 1f;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleLayer;

    private GuardAI guardAI;
    private PlayerStatus playerStatus;
    private bool playerDetected = false;
    private float alertTimeLeft;

    public bool IsPlayerDetected => playerDetected;
    public Transform PlayerTarget => playerTarget;
    public Vector2 LastSeenPosition { get; private set; }

    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public LayerMask ObstacleLayer => obstacleLayer;

    public float TimeToEscape => timeToEscape;
    public float AlertTimeLeft => alertTimeLeft;

    private void Awake()
    {
        guardAI = GetComponent<GuardAI>();
        alertTimeLeft = timeToEscape;

        if (playerTarget == null)
        {
            PlayerStatus found = FindFirstObjectByType<PlayerStatus>();

            if (found != null)
            {
                playerTarget = found.transform;
            }
            else
            {
                GameObject tagged = GameObject.FindWithTag("Player");
                if (tagged != null) playerTarget = tagged.transform;
            }
        }

        if (playerTarget != null)
            playerStatus = playerTarget.GetComponent<PlayerStatus>();

        if (playerTarget == null)
            Debug.LogWarning("GuardVision: гравця не знайдено (поле Player Target)!", this);
    }

    private void Update()
    {
        if (playerTarget == null)
            return;

        if (playerStatus != null && (playerStatus.IsCaught || playerStatus.HasWon))
            return;

        bool visible = CanSeePlayer();
        SetDetection(visible);

        if (visible)
        {
            LastSeenPosition = playerTarget.position;
            alertTimeLeft -= Time.deltaTime;

            if (alertTimeLeft <= 0f)
            {
                alertTimeLeft = 0f;

                if (playerStatus != null)
                    playerStatus.Caught();
                else
                    Debug.LogWarning("На гравці немає PlayerStatus - Game Over не спрацює!", this);
            }
        }
        else
        {
            alertTimeLeft = Mathf.Min(
                timeToEscape,
                alertTimeLeft + recoverSpeed * Time.deltaTime);
        }
    }

    private bool CanSeePlayer()
    {
        if (playerStatus != null && playerStatus.IsInShadow)
            return false;

        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)playerTarget.position - origin;
        float distance = toPlayer.magnitude;

        if (distance > viewDistance)
            return false;

        Vector2 directionToPlayer = toPlayer / Mathf.Max(distance, 0.0001f);

        float angle = Vector2.Angle(
            guardAI.FacingDirection,
            directionToPlayer
        );
        if (angle > viewAngle / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            origin, directionToPlayer, distance, obstacleLayer);

        return hit.collider == null;
    }

    private void SetDetection(bool detected)
    {
        if (playerDetected == detected)
            return;

        playerDetected = detected;
        Debug.Log(detected ? "Guard detected the player!" : "Guard lost sight of the player.");
    }

    private void OnDrawGizmosSelected()
    {
        GuardAI ai = GetComponent<GuardAI>();
        if (ai == null) return;

        Vector2 facing = ai.FacingDirection;
        if (facing.sqrMagnitude < 0.001f) return;

        float halfAngle = viewAngle / 2f;

        Vector2 left = Quaternion.Euler(0, 0, halfAngle) * facing;
        Vector2 right = Quaternion.Euler(0, 0, -halfAngle) * facing;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
        Gizmos.DrawRay(transform.position, facing * viewDistance);
    }
    
    public void ResetState()
    {
        // Відновлюємо таймер тривоги
        alertTimeLeft = timeToEscape;

        // Скидаємо стан виявлення гравця
        playerDetected = false;

        // Очищаємо останню відому позицію гравця
        LastSeenPosition = Vector2.zero;

        Debug.Log("GuardVision: стан успішно скинуто.");
    }
}