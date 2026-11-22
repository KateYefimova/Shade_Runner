using UnityEngine;

public class ExitGuide : MonoBehaviour
{
    [SerializeField] private Color color = new Color(1f, 0.65f, 0.15f, 1f);
    [SerializeField] private float arrowDistance = 0.9f;
    [SerializeField] private float hideArrowWithin = 2.5f;
    [SerializeField] private int sortingOrder = 2;

    private Collider2D area;
    private Transform player;
    private SpriteRenderer marker;
    private SpriteRenderer arrow;

    private void Start()
    {
        area = GetComponent<Collider2D>();

        PlayerStatus status = FindFirstObjectByType<PlayerStatus>();
        if (status != null) player = status.transform;

        marker = CreateRenderer("ExitMarker", CreateMarkerSprite());
        marker.transform.position = area.bounds.center;
        marker.transform.localScale = new Vector3(area.bounds.size.x, area.bounds.size.y, 1f);

        arrow = CreateRenderer("ExitArrow", CreateArrowSprite());
    }

    private void Update()
    {
        float pulse = 0.55f + 0.45f * Mathf.Sin(Time.time * 4f);
        Color current = new Color(color.r, color.g, color.b, pulse);
        marker.color = current;

        Vector2 toExit = player != null ? (Vector2)(area.bounds.center - player.position) : Vector2.zero;
        bool showArrow = player != null && GameManager.IsGameplayActive && toExit.magnitude > hideArrowWithin;

        arrow.enabled = showArrow;
        if (!showArrow) return;

        Vector2 direction = toExit.normalized;
        arrow.color = current;
        arrow.transform.position = (Vector2)player.position + direction * arrowDistance;
        arrow.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void OnDestroy()
    {
        if (marker != null) Destroy(marker.gameObject);
        if (arrow != null) Destroy(arrow.gameObject);
    }

    private SpriteRenderer CreateRenderer(string objectName, Sprite sprite)
    {
        var renderer = new GameObject(objectName).AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static Sprite CreateMarkerSprite()
    {
        const int size = 32;
        const int length = 9;
        const int thickness = 3;

        Texture2D texture = CreateTexture(size);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool corner = (x < length || x >= size - length) && (y < length || y >= size - length);
                bool edge = x < thickness || x >= size - thickness || y < thickness || y >= size - thickness;
                if (corner && edge) texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateArrowSprite()
    {
        const int size = 16;

        Texture2D texture = CreateTexture(size);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int offset = (int)Mathf.Abs(y - 7.5f);
                if (x >= 11 - offset && x <= 14 - offset) texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
    }

    private static Texture2D CreateTexture(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(new Color[size * size]);
        return texture;
    }
}
