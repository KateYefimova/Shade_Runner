using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private float referenceSpeed = 4f;

    private Animator animator;
    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private string currentState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponentInParent<PlayerMovement>();
        interaction = GetComponentInParent<PlayerInteraction>();
    }

    private void Update()
    {
        bool moving = movement.IsMoving;
        bool carrying = interaction != null && interaction.IsHoldingBox;

        string state = (carrying ? "carry_" : "") + (moving ? "walk_" : "idle_") + DirectionName(movement.FacingDirection);
        if (state != currentState)
        {
            animator.Play(state);
            currentState = state;
        }

        animator.speed = moving ? Mathf.Clamp(movement.Velocity.magnitude / referenceSpeed, 0.5f, 1.5f) : 1f;
    }

    private static string DirectionName(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x > 0f ? "east" : "west";
        return direction.y > 0f ? "north" : "south";
    }
}
