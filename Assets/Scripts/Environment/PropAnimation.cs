using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PropAnimation : MonoBehaviour
{
    public enum Mode { Frames, Blink, Hover }

    [SerializeField] private Mode mode = Mode.Frames;

    [Header("Frames")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameTime = 0.15f;

    [Header("Blink / Hover")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float amount = 0.25f;

    private SpriteRenderer sr;
    private Color baseColor;
    private Vector3 startPos;
    private float offset;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        startPos = transform.localPosition;
        // щоб однакові предмети не блимали синхронно
        offset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float t = Time.time + offset;

        switch (mode)
        {
            case Mode.Frames:
                if (frames != null && frames.Length > 0)
                    sr.sprite = frames[(int)(t / frameTime) % frames.Length];
                break;

            case Mode.Blink:
                float k = 1f - amount * (Mathf.Sin(t * speed) * 0.5f + 0.5f);
                sr.color = new Color(baseColor.r * k, baseColor.g * k, baseColor.b * k, baseColor.a);
                break;

            case Mode.Hover:
                transform.localPosition = startPos + Vector3.up * Mathf.Sin(t * speed) * amount;
                break;
        }
    }
}
