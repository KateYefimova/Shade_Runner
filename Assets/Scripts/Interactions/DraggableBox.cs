using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DraggableBox : MonoBehaviour
{
    private Rigidbody2D rb;
    private DistanceJoint2D grabJoint;

    public bool IsGrabbed => grabJoint != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
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

        Debug.Log("Box grabbed");
    }

    public void Release()
    {
        if (grabJoint == null)
            return;

        Destroy(grabJoint);
        grabJoint = null;

        Debug.Log("Box released");
    }

    private void OnDisable()
    {
        Release();
    }
}