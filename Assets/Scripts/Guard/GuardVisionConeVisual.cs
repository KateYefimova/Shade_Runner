
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVisionConeVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuardVision vision;
    [SerializeField] private GuardPatrol patrol;

    [Header("Cone")]
    [SerializeField, Range(10, 100)] private int segments = 40;

    [Header("Colors")]
    [SerializeField] private Color patrolColor =
        new Color(1f, 0.8f, 0.1f, 0.2f);

    [SerializeField] private Color detectedColor =
        new Color(1f, 0.05f, 0.05f, 0.5f);

    [Header("Blink")]
    [SerializeField] private float blinkSpeed = 8f;
    [SerializeField] private float minAlpha = 0.1f;
    [SerializeField] private float maxAlpha = 0.65f;

    private Mesh coneMesh;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorProperty =
        Shader.PropertyToID("_Color");

    private void Awake()
    {
        Debug.Log("Vision Cone Visual Awake");

        coneMesh = new Mesh();
        coneMesh.name = "Guard Vision Cone";

        GetComponent<MeshFilter>().mesh = coneMesh;
        meshRenderer = GetComponent<MeshRenderer>();

        propertyBlock = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        if (vision == null || patrol == null)
        {
            Debug.LogWarning("Vision or Patrol reference is missing!");
            return;
        }

        UpdateCone();
        UpdateColor();
    }
    

    private void UpdateCone()
    {
        Vector2 direction = patrol.FacingDirection;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        direction.Normalize();

        Vector2 localDirection =
            transform.InverseTransformDirection(direction);

        float centerAngle = Mathf.Atan2(
            localDirection.y,
            localDirection.x
        ) * Mathf.Rad2Deg;

        float startAngle = centerAngle - vision.ViewAngle / 2f;
        float angleStep = vision.ViewAngle / segments;

        float distance = vision.ViewDistance;

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (startAngle + angleStep * i)
                * Mathf.Deg2Rad;

            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
        }

        for (int i = 0; i < segments; i++)
        {
            int triangleIndex = i * 3;

            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 2;
            triangles[triangleIndex + 2] = i + 1;
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateBounds();
    }

    private void UpdateColor()
    {
        bool detected = vision.IsPlayerDetected;

        Color color = patrolColor;

        if (detected)
        {
            float blink = Mathf.PingPong(
                Time.time * blinkSpeed,
                1f
            );

            color = detectedColor;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, blink);
        }

        meshRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(BaseColor, color);
        propertyBlock.SetColor(ColorProperty, color);

        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
