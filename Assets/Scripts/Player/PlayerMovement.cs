using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sneakSpeed = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float heldBoxSpeedMultiplier = 0.6f;

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 45f;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 currentVelocity;
    private bool isHoldingBox;
    private bool inputEnabled = true;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public bool InputEnabled
    {
        get => inputEnabled && GameManager.IsGameplayActive;
        set => inputEnabled = value;
    }
    public bool IsSneaking { get; private set; }
    public bool IsMoving => currentVelocity.sqrMagnitude > 0.01f;
    public Vector2 Velocity => currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetHoldingBox(bool holding)
    {
        isHoldingBox = holding;
    }

    public void ResetMotion()
    {
        input = Vector2.zero;
        currentVelocity = Vector2.zero;
        isHoldingBox = false;
        IsSneaking = false;
        FacingDirection = Vector2.down;
        rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        input = Vector2.zero;
        IsSneaking = false;

        if (!InputEnabled || Keyboard.current == null)
            return;

        var kb = Keyboard.current;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y -= 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;

        input = input.normalized;

        IsSneaking = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;

        if (input.sqrMagnitude > 0.01f && !isHoldingBox)
            FacingDirection = input;
    }

    private void FixedUpdate()
    {
        float speed = IsSneaking ? sneakSpeed : walkSpeed;
        if (isHoldingBox)
            speed *= heldBoxSpeedMultiplier;

        Vector2 targetVelocity = InputEnabled ? input * speed : Vector2.zero;

        float rate = targetVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration;

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime);

        rb.linearVelocity = currentVelocity;
    }
}
