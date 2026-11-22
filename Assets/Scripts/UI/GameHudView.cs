using System;
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
    private Camera worldCamera;

    private void Awake()
    {
        hintRect = (RectTransform)hintRoot.transform;
        hintDefaultPosition = hintRect.anchoredPosition;
        worldCamera = Camera.main;

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
    }

    public void SetHint(string hint, Transform target = null)
    {
        bool visible = !string.IsNullOrEmpty(hint);
        hintRoot.SetActive(visible);
        hintTarget = visible ? target : null;

        if (!visible) return;

        hintText.text = hint;
        if (hintTarget == null)
            hintRect.anchoredPosition = hintDefaultPosition;
        else
            FollowHintTarget();
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

        Vector3 anchor = hintTarget.position;
        Renderer targetRenderer = hintTarget.GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
            anchor = new Vector3(targetRenderer.bounds.center.x, targetRenderer.bounds.max.y, 0f);

        Vector3 screen = worldCamera.WorldToScreenPoint(anchor + Vector3.up * hintOffset);
        Vector2 half = Vector2.Scale(hintRect.rect.size, hintRect.lossyScale) * 0.5f;
        screen.x = Mathf.Clamp(screen.x, half.x, Screen.width - half.x);
        screen.y = Mathf.Clamp(screen.y, 0f, Screen.height - half.y * 2f);
        hintRect.position = screen;
    }

    public void ShowState(GameState state, string defeatReason)
    {
        pauseScreen.SetActive(state == GameState.Paused);
        victoryScreen.SetActive(state == GameState.Won);
        defeatScreen.SetActive(state == GameState.Lost);
        pauseButton.gameObject.SetActive(state == GameState.Playing);
        defeatReasonText.text = defeatReason;
    }

    private static void Bind(Button button, Action action)
    {
        if (button != null) button.onClick.AddListener(() => action());
    }
}
