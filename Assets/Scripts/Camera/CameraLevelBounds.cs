using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(1000)]
public class CameraLevelBounds : MonoBehaviour
{
    [SerializeField] private Tilemap levelTilemap;
    [SerializeField] private float padding = 0f;
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f);

    private Camera cam;
    private Bounds levelBounds;
    private bool hasBounds;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;

        if (levelTilemap == null)
        {
            GameObject floor = GameObject.Find("Floor");
            if (floor != null) levelTilemap = floor.GetComponent<Tilemap>();
        }

        if (levelTilemap != null)
            CalculateBounds();
    }

    private void CalculateBounds()
    {
        levelTilemap.CompressBounds();
        Bounds local = levelTilemap.localBounds;
        Vector3 min = levelTilemap.transform.TransformPoint(local.min);
        Vector3 max = levelTilemap.transform.TransformPoint(local.max);

        levelBounds = new Bounds((min + max) / 2f, max - min);
        levelBounds.Expand(new Vector3(padding * 2f, padding * 2f, 0f));
        hasBounds = true;
    }

    private void LateUpdate()
    {
        if (!hasBounds || !cam.orthographic) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 position = transform.position;
        position.x = ClampAxis(position.x, levelBounds.min.x, levelBounds.max.x, halfWidth);
        position.y = ClampAxis(position.y, levelBounds.min.y, levelBounds.max.y, halfHeight);
        transform.position = position;
    }

    private static float ClampAxis(float value, float min, float max, float halfView)
    {
        if (max - min <= halfView * 2f)
            return (min + max) / 2f;

        return Mathf.Clamp(value, min + halfView, max - halfView);
    }
}
