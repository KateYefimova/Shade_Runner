using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStatus : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color shadowColor = new Color(0.55f, 0.55f, 0.65f, 1f);

    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private Color normalColor = Color.white;

    public bool IsInShadow { get; private set; }
    public bool IsCaught { get; private set; }
    public bool HasWon { get; private set; }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;
    }

    public void SetInShadow(bool inShadow)
    {
        if (IsCaught || HasWon || IsInShadow == inShadow)
            return;

        IsInShadow = inShadow;

        if (spriteRenderer != null)
            spriteRenderer.color = inShadow ? shadowColor : normalColor;
    }

    public void Caught()
    {
        if (IsCaught || HasWon)
            return;

        IsCaught = true;
        Freeze();
        Debug.Log("Player caught!");
        GameManager.Instance?.Lose();
    }

    public void Win()
    {
        if (IsCaught || HasWon)
            return;

        HasWon = true;
        Freeze();
        Debug.Log("Player reached the exit!");
        GameManager.Instance?.Win();
    }

    private void Freeze()
    {
        movement.InputEnabled = false;

        if (interaction != null)
            interaction.ReleaseHeld();
    }
}