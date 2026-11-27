using UnityEngine;
using UnityEngine.UI;

public class ResultStatsView : MonoBehaviour
{
    private static Sprite starSprite;

    private readonly Image[] stars = new Image[3];
    private Text statsText;
    private GameObject starsRow;

    private static readonly Color FilledStar = new Color(1f, 0.82f, 0.29f);
    private static readonly Color EmptyStar = new Color(0.25f, 0.25f, 0.35f);

    public void Build(Transform window, int siblingIndex)
    {
        starsRow = UiFactory.CreateRect("Stars", window).gameObject;
        starsRow.transform.SetSiblingIndex(siblingIndex);
        UiFactory.SetHeight(starsRow, 64f);

        HorizontalLayoutGroup row = starsRow.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 12f;
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = false;
        row.childControlHeight = false;
        row.childForceExpandWidth = false;

        for (int i = 0; i < stars.Length; i++)
        {
            RectTransform star = UiFactory.CreateRect("Star", starsRow.transform);
            star.sizeDelta = new Vector2(56f, 56f);
            stars[i] = star.gameObject.AddComponent<Image>();
            stars[i].sprite = StarSprite();
            stars[i].raycastTarget = false;
        }

        statsText = UiFactory.CreateText(window, "Stats", 24, new Color(0.85f, 0.9f, 1f), TextAnchor.MiddleCenter);
        statsText.transform.SetSiblingIndex(siblingIndex + 1);
        statsText.supportRichText = true;
        UiFactory.SetHeight(statsText.gameObject, 130f);
    }

    public void Show(string text, int starCount, bool showStars)
    {
        statsText.text = text;
        starsRow.SetActive(showStars);

        for (int i = 0; i < stars.Length; i++)
            stars[i].color = i < starCount ? FilledStar : EmptyStar;
    }

    private static Sprite StarSprite()
    {
        if (starSprite != null) return starSprite;

        const int size = 64;
        var points = new Vector2[10];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float radius = i % 2 == 0 ? 30f : 12.5f;
            points[i] = new Vector2(size / 2f + Mathf.Cos(angle) * radius, size / 2f + Mathf.Sin(angle) * radius);
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = Inside(points, new Vector2(x + 0.5f, y + 0.5f)) ? Color.white : Color.clear;

        texture.SetPixels(pixels);
        texture.Apply();
        starSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return starSprite;
    }

    private static bool Inside(Vector2[] polygon, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                inside = !inside;
        }
        return inside;
    }
}
