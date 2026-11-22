using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwitchInteractable : MonoBehaviour, IInteractable, IResettable
{
    [SerializeField] private DoorInteractable[] controlledDoors;
    [SerializeField] private bool startsOn = false;
    [Tooltip("Якщо вимкнено - термінал можна використати лише один раз за спробу")]
    [SerializeField] private bool canToggleBack = true;

    [Header("Indicator")]
    [SerializeField] private SpriteRenderer indicator;
    [SerializeField] private Color offColor = new Color(1f, 0.25f, 0.3f);
    [SerializeField] private Color onColor = new Color(0.2f, 1f, 0.5f);

    private bool isOn;

    public Transform HintAnchor => transform;

    private void Awake()
    {
        foreach (DoorInteractable door in controlledDoors)
            door.LockToSwitch(startsOn);

        SetOn(startsOn);
    }

    public bool CanInteract(PlayerInteraction player) => canToggleBack || isOn == startsOn;

    public bool IsLocked(PlayerInteraction player) => false;

    public string GetActionHint(PlayerInteraction player) =>
        isOn ? "Вимкнути термінал" : "Активувати термінал";

    public void Interact(PlayerInteraction player)
    {
        SetOn(!isOn);
    }

    public void ResetState()
    {
        SetOn(startsOn);
    }

    private void SetOn(bool on)
    {
        isOn = on;

        foreach (DoorInteractable door in controlledDoors)
            door.SetOpen(isOn);

        if (indicator != null)
            indicator.color = isOn ? onColor : offColor;
    }

    private void OnDrawGizmosSelected()
    {
        if (controlledDoors == null)
            return;

        Gizmos.color = Color.green;
        foreach (DoorInteractable door in controlledDoors)
        {
            if (door != null)
                Gizmos.DrawLine(transform.position, door.transform.position);
        }
    }
}
