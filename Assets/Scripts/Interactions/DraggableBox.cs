using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DraggableBox : MonoBehaviour, IInteractable, IResettable
{
    private Rigidbody2D rb;
    private DistanceJoint2D grabJoint;

    private Vector2 startPosition;
    private float startRotation;

    public bool IsGrabbed => grabJoint != null;
    public Transform HintAnchor => transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

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

        grabJoint = gameObject.AddComponent<DistanceJoint2D>();

        grabJoint.connectedBody = player;
        grabJoint.autoConfigureConnectedAnchor = false;
        grabJoint.anchor = Vector2.zero;
        grabJoint.connectedAnchor = Vector2.zero;

        grabJoint.autoConfigureDistance = false;
        grabJoint.distance = Vector2.Distance(
            rb.position,
            player.position
        );

        grabJoint.maxDistanceOnly = true;
        grabJoint.enableCollision = true;
    }

    public void Release()
    {
        if (grabJoint == null)
            return;

        Destroy(grabJoint);
        grabJoint = null;
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
