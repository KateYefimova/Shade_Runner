using UnityEngine;

public class ExitGuide : MonoBehaviour
{
    [SerializeField] private Color color = new Color(1f, 0.65f, 0.15f, 1f);
    [SerializeField] private int sortingOrder = 2;

    private Collider2D area;
    private SpriteRenderer marker;

    private void Start()
    {
        area = GetComponent<Collider2D>();

        marker = new GameObject("ExitMarker").AddComponent<SpriteRenderer>();
        marker.sprite = CreateMarkerSprite();
        marker.sortingOrder = sortingOrder;
        marker.transform.position = area.bounds.center;
        marker.transform.localScale = new Vector3(area.bounds.size.x, area.bounds.size.y, 1f);
    }

    private void Update()
    {
        float pulse = 0.55f + 0.45f * Mathf.Sin(Time.time * 4f);
        marker.color = new Color(color.r, color.g, color.b, pulse);
    }

    private void OnDestroy()
    {
        if (marker != null) Destroy(marker.gameObject);
    }

    private static Sprite CreateMarkerSprite()
    {
        const int size = 32;
        const int length = 9;
        const int thickness = 3;

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color[size * size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool corner = (x < length || x >= size - length) && (y < length || y >= size - length);
                bool edge = x < thickness || x >= size - thickness || y < thickness || y >= size - thickness;
                if (corner && edge) pixels[y * size + x] = Color.white;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
