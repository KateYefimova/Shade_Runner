using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatus : MonoBehaviour, IResettable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color shadowColor = new Color(0.55f, 0.55f, 0.65f, 1f);

    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private Rigidbody2D rb;
    private Color normalColor = Color.white;
    private Vector3 startPosition;
    private Quaternion startRotation;

    public bool IsInShadow { get; private set; }
    public bool IsCaught { get; private set; }
    public bool HasWon { get; private set; }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        rb = GetComponent<Rigidbody2D>();

        if (GetComponent<PlayerFootsteps>() == null)
            gameObject.AddComponent<PlayerFootsteps>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void SetInShadow(bool inShadow)
    {
        if (IsCaught || HasWon || IsInShadow == inShadow) return;

        IsInShadow = inShadow;
        if (spriteRenderer != null)
            spriteRenderer.color = inShadow ? shadowColor : normalColor;
    }

    public void Caught()
    {
        if (IsCaught || HasWon) return;

        IsCaught = true;
        Freeze();
        GameManager.Instance?.Lose(LoseReason.Caught);
    }

    public void Win()
    {
        if (IsCaught || HasWon) return;

        HasWon = true;
        Freeze();
        GameManager.Instance?.Win();
    }

    public void ResetState()
    {
        if (interaction != null)
            interaction.ReleaseHeld();

        transform.SetPositionAndRotation(startPosition, startRotation);
        if (rb != null)
        {
            rb.position = startPosition;
            rb.linearVelocity = Vector2.zero;
        }

        movement.ResetMotion();
        movement.InputEnabled = true;

        IsCaught = false;
        HasWon = false;
        IsInShadow = false;

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    private void Freeze()
    {
        movement.InputEnabled = false;
        if (interaction != null)
            interaction.ReleaseHeld();
    }
}
