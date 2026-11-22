using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsView : MonoBehaviour
{
    [SerializeField] private float rowHeight = 44f;
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = new Color(0.92f, 0.95f, 1f);
    [SerializeField] private Color trackColor = new Color(0.08f, 0.07f, 0.16f);
    [SerializeField] private Color fillColor = new Color(0.10f, 0.95f, 1f);
    [SerializeField] private Color handleColor = new Color(1f, 0.20f, 0.75f);

    private static readonly (VolumeChannel channel, string label)[] Rows =
    {
        (VolumeChannel.Master, "Загальна"),
        (VolumeChannel.Music, "Музика"),
        (VolumeChannel.Sfx, "Ефекти"),
        (VolumeChannel.Ui, "Інтерфейс")
    };

    private readonly Slider[] sliders = new Slider[Rows.Length];
    private Font font;
    private Transform container;
    private int insertIndex = -1;
    private bool built;

    public event Action<VolumeChannel, float> VolumeChanged;

    private void Start()
    {
        EnsureBuilt();
    }

    public void SetContainer(Transform target, int siblingIndex)
    {
        container = target;
        insertIndex = siblingIndex;
    }

    public void SetValue(VolumeChannel channel, float value)
    {
        EnsureBuilt();

        for (int i = 0; i < Rows.Length; i++)
            if (Rows[i].channel == channel)
                sliders[i].SetValueWithoutNotify(value);
    }

    private void EnsureBuilt()
    {
        if (built) return;
        built = true;

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (container == null) container = FindContainer();

        for (int i = 0; i < Rows.Length; i++)
        {
            VolumeChannel channel = Rows[i].channel;
            sliders[i] = CreateRow(Rows[i].label);
            if (insertIndex >= 0) sliders[i].transform.parent.SetSiblingIndex(insertIndex + i);
            sliders[i].onValueChanged.AddListener(value => VolumeChanged?.Invoke(channel, value));
        }
    }

    private Slider CreateRow(string label)
    {
        RectTransform row = CreateRect("Volume_" + label, container);
        LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = rowHeight;
        layout.minHeight = rowHeight;

        HorizontalLayoutGroup group = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.spacing = 16f;
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        RectTransform labelRect = CreateRect("Label", row);
        Text text = labelRect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = label;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;
        labelRect.gameObject.AddComponent<LayoutElement>().preferredWidth = 180f;

        RectTransform sliderRect = CreateRect("Slider", row);
        sliderRect.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        return CreateSlider(sliderRect);
    }

    private Slider CreateSlider(RectTransform root)
    {
        RectTransform track = CreateRect("Track", root);
        track.anchorMin = new Vector2(0f, 0.35f);
        track.anchorMax = new Vector2(1f, 0.65f);
        track.offsetMin = track.offsetMax = Vector2.zero;
        track.gameObject.AddComponent<Image>().color = trackColor;

        RectTransform fillArea = CreateRect("FillArea", track);
        Stretch(fillArea);
        RectTransform fill = CreateRect("Fill", fillArea);
        Stretch(fill);
        fill.gameObject.AddComponent<Image>().color = fillColor;

        RectTransform handleArea = CreateRect("HandleArea", root);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);
        RectTransform handle = CreateRect("Handle", handleArea);
        handle.sizeDelta = new Vector2(20f, 0f);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = handleColor;

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    private Transform FindContainer()
    {
        if (GetComponent<VerticalLayoutGroup>() != null) return transform;

        VerticalLayoutGroup layout = GetComponentInChildren<VerticalLayoutGroup>(true);
        return layout != null ? layout.transform : transform;
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
