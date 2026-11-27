using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
public class GuardDebug : MonoBehaviour
{
    [Tooltip("Якщо за кадр охоронця зсунуло більше ніж на стільки, пишемо попередження.")]
    [SerializeField] private float jumpThreshold = 0.5f;

    private AIPath ai;
    private GuardChase chase;

    private Vector3 lastPos;
    private bool lastStopped;
    private Vector3 lastDestination;
    private string lastState;
    private Collider2D col;
    private float stuckTimer;
    private bool stuckLogged;
    private readonly Collider2D[] contacts = new Collider2D[8];

    private void Awake()
    {
        ai = GetComponent<AIPath>();
        chase = GetComponent<GuardChase>();
        lastPos = transform.position;
        lastStopped = ai.isStopped;
        lastDestination = ai.destination;
        col = GetComponent<Collider2D>();
    }
    
    private void Update()
    {
        bool shouldMove = !ai.isStopped && !ai.pathPending &&
                          ai.hasPath && !ai.reachedEndOfPath;

        if (shouldMove && ai.velocity.magnitude < 0.1f)
            stuckTimer += Time.deltaTime;
        else
        {
            stuckTimer = 0f;
            stuckLogged = false;
        }

        if (stuckTimer > 0.5f && !stuckLogged)
        {
            stuckLogged = true;

            var filter = new ContactFilter2D { useTriggers = false };
            int n = col.GetContacts(filter, contacts);
            string list = "";
            for (int i = 0; i < n; i++)
                list += $"{contacts[i].name} (layer {LayerMask.LayerToName(contacts[i].gameObject.layer)}), ";

            var rb2d = GetComponent<Rigidbody2D>();
            Debug.LogWarning(
                $"[GuardDebug] ЗАСТРЯГ. Торкається: {(n == 0 ? "нічого" : list)}" +
                $" | bodyType: {rb2d.bodyType} | remaining: {ai.remainingDistance:F2}" +
                $" | pos: {transform.position} | dest: {ai.destination} | steer: {ai.steeringTarget}",
                this);
        }
    }

    private void LateUpdate()
    {
        float moved = (transform.position - lastPos).magnitude;
        if (moved > jumpThreshold)
        {
            Debug.LogWarning(
                $"[GuardDebug] Стрибок {moved:F2} за кадр | dt: {Time.deltaTime:F3} | " +
                $"час після завантаження сцени: {Time.timeSinceLevelLoad:F1}с | " +
                $"з {lastPos} в {transform.position} | " +
                $"isStopped: {ai.isStopped}, hasPath: {ai.hasPath}", this);
        }
        lastPos = transform.position;

        if (ai.isStopped != lastStopped)
        {
            Debug.Log($"[GuardDebug] isStopped -> {ai.isStopped}", this);
            lastStopped = ai.isStopped;
        }

        if ((ai.destination - lastDestination).sqrMagnitude > 0.01f)
        {
            Debug.Log($"[GuardDebug] destination -> {ai.destination}", this);
            lastDestination = ai.destination;
        }

        if (chase != null && chase.CurrentStateName != lastState)
        {
            Debug.Log($"[GuardDebug] стан -> {chase.CurrentStateName}", this);
            lastState = chase.CurrentStateName;
        }
    }

    private void OnGUI()
    {
        if (ai == null || Camera.main == null)
            return;

        Vector3 sp = Camera.main.WorldToScreenPoint(transform.position);
        string state = chase != null ? chase.CurrentStateName : "-";

        string text =
            $"state: {state}\n" +
            $"stopped: {ai.isStopped}  pending: {ai.pathPending}\n" +
            $"hasPath: {ai.hasPath}\n" +
            $"reachedEnd: {ai.reachedEndOfPath}  reachedDest: {ai.reachedDestination}\n" +
            $"remaining: {ai.remainingDistance:F2}\n" +
            $"speed: {ai.velocity.magnitude:F2}";

        GUI.Label(new Rect(sp.x + 20, Screen.height - sp.y - 40, 300, 120), text);
    }

    private void OnDrawGizmos()
    {
        if (ai == null)
            ai = GetComponent<AIPath>();
        if (ai == null)
            return;

        // Пурпурна: куди AIPath думає, що йому йти (destination)
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, ai.destination);
        Gizmos.DrawWireSphere(ai.destination, 0.2f);

        // Жовта: поточна проміжна точка шляху
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, ai.steeringTarget);

        // Блакитна: фактична швидкість
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, ai.velocity);
    }
}