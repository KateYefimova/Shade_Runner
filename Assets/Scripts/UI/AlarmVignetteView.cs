using UnityEngine;
using UnityEngine.UI;

public class AlarmVignetteView : MonoBehaviour
{
    [SerializeField] private float threshold = 0.6f;
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private Color color = new Color(1f, 0.05f, 0.15f, 0.75f);

    private Image image;
    private float alarm;
    private bool visible = true;

    public void Build(Transform canvas)
    {
        RectTransform rect = UiFactory.CreateRect("AlarmVignette", canvas);
        rect.SetAsFirstSibling();
        UiFactory.Stretch(rect);

        image = rect.gameObject.AddComponent<Image>();
        image.sprite = CreateVignetteSprite();
        image.raycastTarget = false;
        image.color = Color.clear;
    }

    public void SetAlarm(float value) => alarm = value;

    public void SetVisible(bool value) => visible = value;

    private void Update()
    {
        if (image == null) return;

        float intensity = visible ? Mathf.InverseLerp(threshold, 1f, alarm) : 0f;
        float pulse = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
        image.color = new Color(color.r, color.g, color.b, color.a * intensity * pulse);
    }

    private static Sprite CreateVignetteSprite()
    {
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - size / 2f) / (size / 2f);
                float dy = Mathf.Abs(y + 0.5f - size / 2f) / (size / 2f);
                float edge = Mathf.Max(dx, dy);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, edge));
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
