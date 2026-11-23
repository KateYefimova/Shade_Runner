using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour, IInteractionHintSource
{
    private const string InteractKeyLabel = "[E] ";
    private const float LineOfSightSkin = 0.05f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [Tooltip("Стіни, крізь які не можна взаємодіяти (якщо пусто - rooms)")]
    [SerializeField] private LayerMask blockingLayers;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private Rigidbody2D playerRb;
    private PlayerMovement movement;
    private DraggableBox heldBox;

    private readonly Collider2D[] hitBuffer = new Collider2D[16];
    private readonly RaycastHit2D[] losBuffer = new RaycastHit2D[8];
    private ContactFilter2D interactFilter;
    private ContactFilter2D blockFilter;

    private IInteractable currentTarget;
    private SpriteRenderer highlighted;
    private Color originalColor;

    public bool IsHoldingBox => heldBox != null;

    public string CurrentHint
    {
        get
        {
            if (!movement.InputEnabled) return string.Empty;
            if (heldBox != null) return InteractKeyLabel + "Відпустити контейнер";
            if (currentTarget == null) return string.Empty;

            string hint = currentTarget.GetActionHint(this);
            return currentTarget.IsLocked(this) ? hint : InteractKeyLabel + hint;
        }
    }

    public Transform CurrentHintTarget
    {
        get
        {
            if (!movement.InputEnabled) return null;
            if (heldBox != null) return heldBox.transform;
            return currentTarget?.HintAnchor;
        }
    }

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();

        if (blockingLayers.value == 0)
            blockingLayers = LayerMask.GetMask("rooms");

        interactFilter = new ContactFilter2D();
        interactFilter.SetLayerMask(interactableLayers);
        interactFilter.useTriggers = true;

        blockFilter = new ContactFilter2D();
        blockFilter.SetLayerMask(blockingLayers);
        blockFilter.useTriggers = false;
    }

    private void Update()
    {
        if (heldBox != null && !heldBox.IsGrabbed)
            ReleaseHeld();

        if (!movement.InputEnabled)
        {
            currentTarget = null;
            ClearHighlight();
            return;
        }

        bool ePressed = Keyboard.current != null &&
                        Keyboard.current.eKey.wasPressedThisFrame;

        if (heldBox != null)
        {
            currentTarget = null;
            ClearHighlight();
            if (ePressed)
                ReleaseHeld();
            return;
        }

        FindNearestTarget();
        UpdateHighlight();

        if (ePressed && currentTarget != null)
            currentTarget.Interact(this);
    }

    public void StartHolding(DraggableBox box)
    {
        if (box == null || heldBox != null)
            return;

        box.Grab(playerRb);
        heldBox = box;
        movement.SetHoldingBox(true);
    }

    public void ReleaseHeld()
    {
        if (heldBox != null)
        {
            heldBox.Release();
            heldBox = null;
        }
        movement.SetHoldingBox(false);
    }

    private void FindNearestTarget()
    {
        currentTarget = null;

        int count = Physics2D.OverlapCircle(
            transform.position, interactionDistance, interactFilter, hitBuffer);

        float minDistance = float.MaxValue;
        Vector2 pos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hitBuffer[i];
            IInteractable interactable = col.GetComponentInParent<IInteractable>();

            if (interactable == null || !interactable.CanInteract(this))
                continue;

            Vector2 closest = col.ClosestPoint(pos);
            float distance = Vector2.Distance(pos, closest);

            if (distance >= minDistance)
                continue;

            if (!HasLineOfSight(pos, col, closest, interactable))
                continue;

            minDistance = distance;
            currentTarget = interactable;
        }
    }

    private bool HasLineOfSight(Vector2 from, Collider2D col, Vector2 closest, IInteractable target)
    {
        return IsPathClear(from, closest, target) ||
               IsPathClear(from, col.bounds.center, target);
    }

    private bool IsPathClear(Vector2 from, Vector2 to, IInteractable target)
    {
        Vector2 delta = to - from;
        float length = delta.magnitude - LineOfSightSkin;
        if (length <= 0f)
            return true;

        int hits = Physics2D.Raycast(from, delta.normalized, blockFilter, losBuffer, length);
        for (int i = 0; i < hits; i++)
        {
            Collider2D hit = losBuffer[i].collider;
            if (hit.attachedRigidbody == playerRb)
                continue;
            if (hit.GetComponentInParent<IInteractable>() == target)
                continue;
            return false;
        }
        return true;
    }

    private void UpdateHighlight()
    {
        SpriteRenderer sr = currentTarget != null
            ? currentTarget.HintAnchor.GetComponentInChildren<SpriteRenderer>()
            : null;

        if (sr == highlighted)
            return;

        ClearHighlight();

        if (sr != null)
        {
            highlighted = sr;
            originalColor = sr.color;
            sr.color = highlightColor;
        }
    }

    private void ClearHighlight()
    {
        if (highlighted != null)
            highlighted.color = originalColor;

        highlighted = null;
    }

    private void OnDisable()
    {
        currentTarget = null;
        ClearHighlight();
        ReleaseHeld();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
