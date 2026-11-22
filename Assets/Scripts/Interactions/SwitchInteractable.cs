using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwitchInteractable : MonoBehaviour, IInteractable, IResettable
{
    [Header("Прохід")]
    [Tooltip("Двері, якими керує термінал. Увімкнено = відчинені.")]
    [SerializeField] private DoorInteractable[] controlledDoors = Array.Empty<DoorInteractable>();
    [Tooltip("Чи увімкнений термінал на початку спроби.")]
    [SerializeField] private bool startsOn = false;
    [Tooltip("Якщо вимкнено - термінал спрацьовує лише один раз за спробу.")]
    [SerializeField] private bool canToggleBack = true;

    [Header("Вигляд (необов'язково)")]
    [Tooltip("Окремий спрайт-індикатор, що змінює колір (лампочка, екран).")]
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private Color offColor = new Color(1f, 0.25f, 0.3f, 1f);
    [SerializeField] private Color onColor = new Color(0.2f, 1f, 0.5f, 1f);
    [Tooltip("Основний спрайт, у якого міняємо картинку.")]
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    [Header("Підказки")]
    [SerializeField] private string activateHint = "Активувати термінал";
    [SerializeField] private string deactivateHint = "Вимкнути термінал";

    private bool isOn;

    public bool IsOn => isOn;
    public Transform HintAnchor => transform;

    public event Action<bool> Switched;

    private void Awake()
    {
        isOn = startsOn;

        foreach (DoorInteractable door in controlledDoors)
        {
            if (door != null)
                door.AssignController(this, startsOn);
        }

        ApplyVisuals();
    }

    public bool CanInteract(PlayerInteraction player) => canToggleBack || isOn == startsOn;

    public bool IsLocked(PlayerInteraction player) => false;

    public string GetActionHint(PlayerInteraction player) =>
        isOn ? deactivateHint : activateHint;

    public void Interact(PlayerInteraction player)
    {
        if (!CanInteract(player))
            return;

        SetOn(!isOn);
        Switched?.Invoke(isOn);
    }

    public void ResetState()
    {
        SetOn(startsOn);
    }

    private void SetOn(bool on)
    {
        isOn = on;

        foreach (DoorInteractable door in controlledDoors)
        {
            if (door != null)
                door.SetOpen(isOn);
        }

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (indicator != null)
            indicator.color = isOn ? onColor : offColor;

        if (body != null)
        {
            Sprite sprite = isOn ? onSprite : offSprite;
            if (sprite != null)
                body.sprite = sprite;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (DoorInteractable door in controlledDoors)
        {
            if (door != null)
                Gizmos.DrawLine(transform.position, door.transform.position);
        }
    }
}
