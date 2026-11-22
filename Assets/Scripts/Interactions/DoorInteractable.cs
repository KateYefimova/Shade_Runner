using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DoorInteractable : MonoBehaviour, IInteractable, IResettable
{
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private bool startsOpen = false;

    private Rigidbody2D rb;
    private float closedAngle;
    private bool isOpen;
    private bool controlledBySwitch;

    public Transform HintAnchor => transform;

    private float TargetAngle => isOpen ? closedAngle + openAngle : closedAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        closedAngle = rb.rotation;
        isOpen = startsOpen;
        SnapToTarget();
    }

    public bool CanInteract(PlayerInteraction player) => true;

    public bool IsLocked(PlayerInteraction player) => controlledBySwitch;

    public string GetActionHint(PlayerInteraction player)
    {
        if (controlledBySwitch)
            return isOpen ? "Відчинено терміналом" : "Зачинено: потрібен термінал";

        return isOpen ? "Зачинити двері" : "Відчинити двері";
    }

    public void Interact(PlayerInteraction player)
    {
        SetOpen(!isOpen);
    }

    // стартовий стан беремо з термінала, щоб після рестарту двері і термінал не розходились
    public void LockToSwitch(bool openAtStart)
    {
        controlledBySwitch = true;
        startsOpen = openAtStart;
        isOpen = openAtStart;

        if (rb != null)
            SnapToTarget();
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
    }

    public void ResetState()
    {
        isOpen = startsOpen;
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        float angle = TargetAngle;
        rb.angularVelocity = 0f;
        rb.rotation = angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FixedUpdate()
    {
        float newAngle = Mathf.MoveTowardsAngle(
            rb.rotation,
            TargetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }
}
