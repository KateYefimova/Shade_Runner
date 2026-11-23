using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DoorInteractable : MonoBehaviour, IInteractable, IResettable
{
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private bool startsOpen = false;

    [Header("Sounds")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private float closedAngle;
    private bool isOpen;
    private bool controlledBySwitch;

    public Transform HintAnchor => transform;

    private float TargetAngle => isOpen ? closedAngle + openAngle : closedAngle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = SfxSource.Create(gameObject);
        closedAngle = rb.rotation;
        isOpen = startsOpen;
        SnapToTarget();
    }

    public bool CanInteract(PlayerInteraction player) => true;

    public bool IsLocked(PlayerInteraction player) => controlledBySwitch;

    public string GetActionHint(PlayerInteraction player)
    {
        if (controlledBySwitch)
            return isOpen ? "Відчинено терміналом" : "Зачинено: потрібен термінал";

        return isOpen ? "Зачинити двері" : "Відчинити двері";
    }

    public void Interact(PlayerInteraction player)
    {
        if (controlledBySwitch)
        {
            Play(lockedSound);
            return;
        }

        SetOpen(!isOpen);
    }

    // стартовий стан беремо з термінала, щоб після рестарту двері і термінал не розходились
    public void LockToSwitch(bool openAtStart)
    {
        controlledBySwitch = true;
        startsOpen = openAtStart;
        isOpen = openAtStart;

        if (rb != null)
            SnapToTarget();
    }

    public void SetOpen(bool open)
    {
        if (isOpen == open)
            return;

        isOpen = open;
        Play(isOpen ? openSound : closeSound);
    }

    private void Play(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void ResetState()
    {
        isOpen = startsOpen;
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        float angle = TargetAngle;
        rb.angularVelocity = 0f;
        rb.rotation = angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FixedUpdate()
    {
        float newAngle = Mathf.MoveTowardsAngle(
            rb.rotation,
            TargetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newAngle);
    }
}
