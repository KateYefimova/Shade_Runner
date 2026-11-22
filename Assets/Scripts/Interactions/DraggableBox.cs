using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DraggableBox : MonoBehaviour, IInteractable, IResettable
{
    [SerializeField] private float maxFollowSpeed = 12f;
    [Tooltip("Ящик застряг і гравець відійшов далі - відпускаємо")]
    [SerializeField] private float breakDistance = 0.8f;
    [SerializeField] private float breakDelay = 0.25f;

    private Rigidbody2D rb;
    private Rigidbody2D holder;
    private Vector2 holdOffset;
    private float stuckTime;

    private float defaultDamping;
    private RigidbodyInterpolation2D defaultInterpolation;

    private Vector2 startPosition;
    private float startRotation;

    public bool IsGrabbed => holder != null;
    public Transform HintAnchor => transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        defaultDamping = rb.linearDamping;
        defaultInterpolation = rb.interpolation;

        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    public bool CanInteract(PlayerInteraction player) => !IsGrabbed;

    public bool IsLocked(PlayerInteraction player) => false;

    public string GetActionHint(PlayerInteraction player) => "Захопити ящик";

    public void Interact(PlayerInteraction player)
    {
        player.StartHolding(this);
    }

    public void Grab(Rigidbody2D player)
    {
        if (IsGrabbed || player == null)
            return;

        holder = player;
        holdOffset = rb.position - player.position;
        stuckTime = 0f;

        rb.linearDamping = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void Release()
    {
        if (holder == null)
            return;

        holder = null;
        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = defaultDamping;
        rb.interpolation = defaultInterpolation;
    }

    private void FixedUpdate()
    {
        if (holder == null)
            return;

        float dt = Time.fixedDeltaTime;
        Vector2 target = holder.position + holder.linearVelocity * dt + holdOffset;
        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude > breakDistance)
        {
            stuckTime += dt;
            if (stuckTime >= breakDelay)
            {
                Release();
                return;
            }
        }
        else
        {
            stuckTime = 0f;
        }

        rb.linearVelocity = Vector2.ClampMagnitude(toTarget / dt, maxFollowSpeed);
    }

    public void ResetState()
    {
        Release();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = startPosition;
        rb.rotation = startRotation;
        transform.SetPositionAndRotation(
            new Vector3(startPosition.x, startPosition.y, transform.position.z),
            Quaternion.Euler(0f, 0f, startRotation));
    }

    private void OnDisable()
    {
        Release();
    }
}
