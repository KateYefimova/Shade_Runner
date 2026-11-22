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

    private void Awake()
    {
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

    public void SetHint(string hint)
    {
        bool visible = !string.IsNullOrEmpty(hint);
        hintRoot.SetActive(visible);
        if (visible) hintText.text = hint;
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
