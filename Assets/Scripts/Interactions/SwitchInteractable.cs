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

    [SerializeField] private AudioClip switchSound;

    private bool isOn;
    private AudioSource audioSource;

    public Transform HintAnchor => transform;

    private void Awake()
    {
        audioSource = SfxSource.Create(gameObject);

        foreach (DoorInteractable door in controlledDoors)
            door.LockToSwitch(startsOn);

        isOn = startsOn;
        UpdateIndicator();
    }

    public bool CanInteract(PlayerInteraction player) => canToggleBack || isOn == startsOn;

    public bool IsLocked(PlayerInteraction player) => false;

    public string GetActionHint(PlayerInteraction player) =>
        isOn ? "Вимкнути термінал" : "Активувати термінал";

    public void Interact(PlayerInteraction player)
    {
        isOn = !isOn;

        foreach (DoorInteractable door in controlledDoors)
            door.SetOpen(isOn);

        if (switchSound != null)
            audioSource.PlayOneShot(switchSound);

        UpdateIndicator();
    }

    // двері скидаються самі, тут тільки стан термінала
    public void ResetState()
    {
        isOn = startsOn;
        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
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
