using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 1.5f;
    [Tooltip("Шари, на яких лежать двері та ящики. За замовчуванням - усі.")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private Rigidbody2D playerRb;
    private PlayerMovement movement;
    private DraggableBox heldBox;

    private readonly Collider2D[] hitBuffer = new Collider2D[16];
    private ContactFilter2D filter;

    private Component currentTarget;
    private SpriteRenderer highlighted;
    private Color originalColor;

    public bool IsHoldingBox => heldBox != null;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();

        filter = new ContactFilter2D();
        filter.SetLayerMask(interactableLayers);
        filter.useTriggers = true;
    }

    private void Update()
    {
        if (!movement.InputEnabled)
        {
            ClearHighlight();
            return;
        }

        bool ePressed = Keyboard.current != null &&
                        Keyboard.current.eKey.wasPressedThisFrame;

        if (heldBox != null)
        {
            ClearHighlight();
            if (ePressed)
                ReleaseHeld();
            return;
        }

        FindNearestTarget();
        UpdateHighlight();

        if (!ePressed || currentTarget == null)
            return;

        if (currentTarget is DoorInteractable door)
        {
            door.Interact();
        }
        else if (currentTarget is DraggableBox box)
        {
            box.Grab(playerRb);
            heldBox = box;
            movement.SetHoldingBox(true);
        }
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
            transform.position, interactionDistance, filter, hitBuffer);

        float minDistance = float.MaxValue;
        Vector2 pos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = hitBuffer[i];

            DoorInteractable door = col.GetComponentInParent<DoorInteractable>();
            DraggableBox box = col.GetComponentInParent<DraggableBox>();

            if (door == null && box == null)
                continue;

            if (box != null && box.IsGrabbed)
                continue;

            float distance = Vector2.Distance(pos, col.ClosestPoint(pos));

            if (distance < minDistance)
            {
                minDistance = distance;
                currentTarget = door != null ? (Component)door : box;
            }
        }
    }

    private void UpdateHighlight()
    {
        SpriteRenderer sr = currentTarget != null
            ? currentTarget.GetComponentInChildren<SpriteRenderer>()
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
        ClearHighlight();
        ReleaseHeld();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}