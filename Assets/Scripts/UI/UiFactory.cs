using UnityEngine;
using UnityEngine.UI;

public static class UiFactory
{
    private static Font font;

    public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static Text CreateText(Transform parent, string name, int size, Color color, TextAnchor alignment)
    {
        Text text = CreateRect(name, parent).gameObject.AddComponent<Text>();
        text.font = Font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    public static LayoutElement SetHeight(GameObject go, float height)
    {
        LayoutElement layout = go.GetComponent<LayoutElement>();
        if (layout == null) layout = go.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;
        return layout;
    }
}
