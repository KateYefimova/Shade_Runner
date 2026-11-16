using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVisionConeVisual : MonoBehaviour
{
    [Header("References (якщо порожні - знайдуться автоматично)")]
    [SerializeField] private GuardVision vision;
    [SerializeField] private GuardPatrol patrol;

    [Header("Cone")]
    [SerializeField, Range(10, 100)] private int segments = 40;
    [SerializeField] private bool clipByObstacles = true;

    [Header("Sorting (якщо конуса не видно - збільшіть Order)")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 5;

    [Header("Colors")]
    [SerializeField] private Color patrolColor = new Color(1f, 0.8f, 0.1f, 0.25f);
    [SerializeField] private Color detectedColor = new Color(1f, 0.05f, 0.05f, 0.5f);

    [Header("Blink")]
    [SerializeField] private float blinkSpeed = 6f;
    [Tooltip("У скільки разів блимає швидше, коли час майже вичерпано")]
    [SerializeField] private float blinkSpeedUp = 3f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.7f;

    private Mesh coneMesh;
    private Material material;
    private Vector3[] vertices;
    private Color[] colors;
    private float blinkPhase;

    private void Awake()
    {
        if (vision == null) vision = GetComponentInParent<GuardVision>();
        if (patrol == null) patrol = GetComponentInParent<GuardPatrol>();

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        material = new Material(shader);
        if (material.HasProperty("_Color"))
            material.color = Color.white;   

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;

        vertices = new Vector3[segments + 2];
        colors = new Color[segments + 2];

        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        coneMesh = new Mesh { name = "Guard Vision Cone" };
        coneMesh.vertices = vertices;
        coneMesh.colors = colors;
        coneMesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = coneMesh;
    }

    private void LateUpdate()
    {
        if (vision == null || patrol == null)
        {
            Debug.LogWarning(
                "GuardVisionConeVisual: не знайдено GuardVision/GuardPatrol!", this);
            return;
        }

        Color color = GetCurrentColor();
        UpdateCone(color);
    }

    private Color GetCurrentColor()
    {
        if (!vision.IsPlayerDetected)
        {
            blinkPhase = 0f;
            return patrolColor;
        }

        float urgency = 1f - Mathf.Clamp01(vision.AlertTimeLeft / vision.TimeToEscape);
        float speed = blinkSpeed * (1f + blinkSpeedUp * urgency);

        blinkPhase += Time.deltaTime * speed;
        float blink = Mathf.PingPong(blinkPhase, 1f);

        Color c = detectedColor;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, blink);
        return c;
    }

    private void UpdateCone(Color color)
    {
        Vector3 originWorld = patrol.transform.position;

        Vector2 facing = patrol.FacingDirection;
        if (facing.sqrMagnitude < 0.001f)
            facing = Vector2.right;

        float centerAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float startAngle = centerAngle - vision.ViewAngle / 2f;
        float angleStep = vision.ViewAngle / segments;

        vertices[0] = transform.InverseTransformPoint(originWorld);
        colors[0] = color;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            float distance = vision.ViewDistance;

            if (clipByObstacles)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    originWorld, dir, distance, vision.ObstacleLayer);

                if (hit.collider != null)
                    distance = hit.distance;
            }

            Vector3 pointWorld = originWorld + (Vector3)(dir * distance);
            vertices[i + 1] = transform.InverseTransformPoint(pointWorld);
            colors[i + 1] = color;
        }

        coneMesh.vertices = vertices;
        coneMesh.colors = colors;
        coneMesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
        if (coneMesh != null) Destroy(coneMesh);
    }
}