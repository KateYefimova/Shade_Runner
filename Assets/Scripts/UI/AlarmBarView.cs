using UnityEngine;
using UnityEngine.UI;

public class AlarmBarView : MonoBehaviour
{
    [SerializeField] private RectTransform fill;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text label;
    [SerializeField] private Gradient colors = DefaultGradient();
    [SerializeField] private float smoothSpeed = 2.5f;

    private float target;
    private float shown;

    public void SetValue(float value, bool instant = false)
    {
        target = Mathf.Clamp01(value);
        if (instant) shown = target;
        Apply();
    }

    private void Update()
    {
        if (Mathf.Approximately(shown, target)) return;

        shown = Mathf.MoveTowards(shown, target, smoothSpeed * Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        if (fill != null) fill.anchorMax = new Vector2(shown, 1f);
        if (fillImage != null) fillImage.color = colors.Evaluate(shown);
        if (label != null) label.text = $"ТРИВОГА {shown * 100f:0}%";
    }

    private static Gradient DefaultGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.10f, 0.95f, 1.00f), 0f),
                new GradientColorKey(new Color(1.00f, 0.80f, 0.15f), 0.55f),
                new GradientColorKey(new Color(1.00f, 0.15f, 0.35f), 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }
}
