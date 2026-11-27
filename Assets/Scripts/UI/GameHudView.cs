using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameHudView : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private AlarmBarView alarmBar;
    [SerializeField] private GameObject hintRoot;
    [SerializeField] private Text hintText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private float hintOffset = 0.3f;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;

    [Header("Victory")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Button victoryRestartButton;

    [Header("Defeat")]
    [SerializeField] private GameObject defeatScreen;
    [SerializeField] private Text defeatReasonText;
    [SerializeField] private Button defeatRestartButton;

    [Header("Menu")]
    [SerializeField] private Button[] menuButtons;

    public event Action PauseClicked;
    public event Action ResumeClicked;
    public event Action RestartClicked;
    public event Action MenuClicked;

    private RectTransform hintRect;
    private Vector2 hintDefaultPosition;
    private Transform hintTarget;
    private PlayerInteraction hintPlayer;
    private bool hintBelow;
    private Camera worldCamera;
    private AlarmVignetteView vignette;
    private ResultStatsView victoryStats;
    private ResultStatsView defeatStats;
    private Text pathHintLabel;
    private CanvasGroup controlsHint;
    private Selectable focusTarget;

    private void Awake()
    {
        hintRect = (RectTransform)hintRoot.transform;
        hintDefaultPosition = hintRect.anchoredPosition;
        worldCamera = Camera.main;
        BuildExtras();

        Bind(pauseButton, () => PauseClicked?.Invoke());
        Bind(resumeButton, () => ResumeClicked?.Invoke());
        Bind(pauseRestartButton, () => RestartClicked?.Invoke());
        Bind(victoryRestartButton, () => RestartClicked?.Invoke());
        Bind(defeatRestartButton, () => RestartClicked?.Invoke());

        foreach (Button button in menuButtons)
            Bind(button, () => MenuClicked?.Invoke());
    }

    public void SetMenuAvailable(bool available)
    {
        foreach (Button button in menuButtons)
            if (button != null) button.gameObject.SetActive(available);
    }

    public void SetAlarm(float value, bool instant = false)
    {
        alarmBar.SetValue(value, instant);
        vignette.SetAlarm(value);
    }

    public void SetPathHint(bool active)
    {
        pathHintLabel.text = active ? "Шлях до виходу показано" : "[H]  шлях до виходу  (-1 зірка)";
        pathHintLabel.color = active ? new Color(0.1f, 0.95f, 1f) : new Color(0.7f, 0.75f, 0.85f, 0.8f);
    }

    public void SetResult(string text, int stars, bool won)
    {
        ResultStatsView stats = won ? victoryStats : defeatStats;
        stats.Show(text, stars, won);
    }

    public void SetHint(string hint, Transform target = null)
    {
        bool visible = !string.IsNullOrEmpty(hint);
        hintRoot.SetActive(visible);
        hintTarget = visible ? target : null;

        if (!visible) return;

        hintText.text = hint;
        FitHintToText();
        if (hintTarget == null)
            hintRect.anchoredPosition = hintDefaultPosition;
        else
            FollowHintTarget();
    }

    private void Update()
    {
        if (focusTarget != null) UiSelection.KeepFocus(focusTarget);
    }

    public void ShowControls(float holdTime, float fadeTime)
    {
        StopAllCoroutines();
        StartCoroutine(FadeControls(holdTime, fadeTime));
    }

    private IEnumerator FadeControls(float holdTime, float fadeTime)
    {
        controlsHint.alpha = 1f;
        yield return new WaitForSecondsRealtime(holdTime);

        for (float time = 0f; time < fadeTime; time += Time.unscaledDeltaTime)
        {
            controlsHint.alpha = 1f - time / fadeTime;
            yield return null;
        }
        controlsHint.alpha = 0f;
    }

    private void LateUpdate()
    {
        if (hintTarget != null)
            FollowHintTarget();
    }

    private void FollowHintTarget()
    {
        if (worldCamera == null) worldCamera = Camera.main;
        if (worldCamera == null) return;

        Bounds bounds = new Bounds(hintTarget.position, Vector3.zero);
        Renderer targetRenderer = hintTarget.GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
            bounds = targetRenderer.bounds;

        bool showBelow = IsPlayerAbove(bounds);
        Vector3 anchor = showBelow
            ? new Vector3(bounds.center.x, bounds.min.y - hintOffset, 0f)
            : new Vector3(bounds.center.x, bounds.max.y + hintOffset, 0f);

        Vector3 screen = worldCamera.WorldToScreenPoint(anchor);
        Vector2 size = Vector2.Scale(hintRect.rect.size, hintRect.lossyScale);
        if (showBelow)
            screen.y -= size.y;

        float half = size.x * 0.5f;
        screen.x = Mathf.Clamp(screen.x, half, Screen.width - half);
        screen.y = Mathf.Clamp(screen.y, 0f, Screen.height - size.y);
        hintRect.position = screen;
    }

    private bool IsPlayerAbove(Bounds targetBounds)
    {
        if (hintPlayer == null)
            hintPlayer = FindFirstObjectByType<PlayerInteraction>();

        if (hintPlayer == null)
            return false;

        const float margin = 0.15f;
        float dy = hintPlayer.transform.position.y - targetBounds.center.y;
        if (dy > margin) hintBelow = true;
        else if (dy < -margin) hintBelow = false;
        return hintBelow;
    }

    private void FitHintToText()
    {
        RectTransform textRect = hintText.rectTransform;
        float padding = -textRect.sizeDelta.x;
        float width = Mathf.Ceil(hintText.preferredWidth) + padding;
        hintRect.sizeDelta = new Vector2(width, hintRect.sizeDelta.y);
    }

    public void ShowState(GameState state, string defeatReason)
    {
        pauseScreen.SetActive(state == GameState.Paused);
        victoryScreen.SetActive(state == GameState.Won);
        defeatScreen.SetActive(state == GameState.Lost);
        pauseButton.gameObject.SetActive(state == GameState.Playing);
        pathHintLabel.gameObject.SetActive(state == GameState.Playing);
        vignette.SetVisible(state == GameState.Playing);
        defeatReasonText.text = defeatReason;

        focusTarget = state switch
        {
            GameState.Paused => resumeButton,
            GameState.Won => victoryRestartButton,
            GameState.Lost => defeatRestartButton,
            _ => null
        };

        if (focusTarget != null) UiSelection.Focus(focusTarget);
        else UiSelection.Clear();
    }

    private void BuildExtras()
    {
        vignette = gameObject.AddComponent<AlarmVignetteView>();
        vignette.Build(transform);

        victoryStats = BuildStats(victoryScreen);
        defeatStats = BuildStats(defeatScreen);

        pathHintLabel = UiFactory.CreateText(hintRoot.transform.parent, "PathHintLabel", 22, Color.white, TextAnchor.LowerLeft);
        RectTransform rect = pathHintLabel.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(28f, 24f);
        rect.sizeDelta = new Vector2(600f, 40f);
        SetPathHint(false);

        controlsHint = BuildControlsHint(hintRoot.transform.parent);
    }

    private static CanvasGroup BuildControlsHint(Transform parent)
    {
        RectTransform panel = UiFactory.CreateRect("ControlsHint", parent);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 130f);
        panel.sizeDelta = new Vector2(1100f, 56f);
        panel.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.03f, 0.09f, 0.8f);

        Text text = UiFactory.CreateText(panel, "Text", 24, new Color(0.85f, 0.9f, 1f), TextAnchor.MiddleCenter);
        UiFactory.Stretch(text.rectTransform);
        text.text = "WASD — рух     Shift — тихий крок     E — дія     H — шлях     Esc — пауза";

        CanvasGroup group = panel.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        return group;
    }

    private static ResultStatsView BuildStats(GameObject screen)
    {
        VerticalLayoutGroup window = screen.GetComponentInChildren<VerticalLayoutGroup>(true);
        ResultStatsView stats = screen.AddComponent<ResultStatsView>();
        stats.Build(window.transform, 2);
        return stats;
    }

    private static void Bind(Button button, Action action)
    {
        if (button != null) button.onClick.AddListener(() => action());
    }
}
