using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    private const float Duration = 0.3f;

    private static ScreenFader instance;

    private CanvasGroup group;
    private bool busy;

    public static bool IsBusy => instance != null && instance.busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject(nameof(ScreenFader));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScreenFader>();
    }

    public static void Run(Action action)
    {
        if (instance == null)
        {
            action();
            return;
        }

        if (!instance.busy)
            instance.StartCoroutine(instance.FadeAround(action));
    }

    private void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        gameObject.AddComponent<GraphicRaycaster>();
        group = gameObject.AddComponent<CanvasGroup>();

        RectTransform cover = UiFactory.CreateRect("Cover", transform);
        UiFactory.Stretch(cover);
        cover.gameObject.AddComponent<Image>().color = new Color(0.01f, 0.01f, 0.03f, 1f);

        group.alpha = 1f;
        StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator FadeAround(Action action)
    {
        busy = true;
        yield return Fade(0f, 1f);
        action();
        yield return null;
        yield return Fade(1f, 0f);
        busy = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        group.blocksRaycasts = true;

        for (float time = 0f; time < Duration; time += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, time / Duration);
            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}
