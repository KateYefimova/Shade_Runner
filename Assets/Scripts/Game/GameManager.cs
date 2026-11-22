using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGameplayActive => Instance == null || Instance.Model.IsPlaying;

    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private float pathHintDuration = 8f;

    public GameSessionModel Model { get; } = new GameSessionModel();
    public bool IsGameOver => Model.IsFinished;
    public bool HasMenu => Application.CanStreamedLevelBeLoaded(menuSceneName);

    private readonly List<IAlarmSource> alarmSources = new List<IAlarmSource>();
    private readonly List<IInteractionHintSource> hintSources = new List<IInteractionHintSource>();
    private float pathHintTimeLeft;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb is IAlarmSource alarm) alarmSources.Add(alarm);
            if (mb is IInteractionHintSource hint) hintSources.Add(hint);
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.escapeKey.wasPressedThisFrame) TogglePause();
            if (kb.rKey.wasPressedThisFrame) Restart();
            if (kb.hKey.wasPressedThisFrame) RequestPathHint();
        }

        if (!Model.IsPlaying) return;

        Model.AddTime(Time.deltaTime);
        UpdatePathHint();
        PollSources();
    }

    public void RequestPathHint()
    {
        if (!Model.IsPlaying) return;

        pathHintTimeLeft = pathHintDuration;
        Model.SetPathHint(true);
    }

    private void UpdatePathHint()
    {
        if (!Model.PathHintActive) return;

        pathHintTimeLeft -= Time.deltaTime;
        if (pathHintTimeLeft <= 0f)
            Model.SetPathHint(false);
    }

    public void Pause()
    {
        if (!Model.IsPlaying) return;

        Model.SetState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (Model.State != GameState.Paused) return;

        Time.timeScale = 1f;
        Model.SetState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (Model.State == GameState.Paused) Resume();
        else Pause();
    }

    public void Win()
    {
        if (Model.IsFinished) return;

        Model.SetHint(string.Empty);
        Model.SetPathHint(false);
        Model.SetResult(BuildResult(true));
        Model.SetState(GameState.Won);
        Time.timeScale = 0f;
    }

    public void Lose(LoseReason reason = LoseReason.Caught)
    {
        if (Model.IsFinished) return;

        if (alarmSources.Count > 0)
        {
            float alarm = ReadAlarm();
            Model.SetAlarm(alarm);
            if (reason == LoseReason.Caught && alarm >= 1f)
                reason = LoseReason.MaxAlarm;
        }

        Model.SetHint(string.Empty);
        Model.SetPathHint(false);
        Model.SetResult(BuildResult(false));
        Model.SetState(GameState.Lost, reason);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb is IResettable resettable)
                resettable.ResetState();
        }

        Physics2D.SyncTransforms();
        Model.ResetForNewAttempt();
    }

    private AttemptResult BuildResult(bool won)
    {
        string key = "best_time_" + SceneManager.GetActiveScene().name;
        float best = PlayerPrefs.GetFloat(key, 0f);
        bool isNewBest = false;
        int stars = 0;

        if (won)
        {
            stars = 3;
            if (Model.MaxAlarm > 0.01f) stars--;
            if (Model.PathHintsUsed > 0) stars--;

            if (best <= 0f || Model.AttemptTime < best)
            {
                best = Model.AttemptTime;
                isNewBest = true;
                PlayerPrefs.SetFloat(key, best);
                PlayerPrefs.Save();
            }
        }

        return new AttemptResult(won, Model.AttemptTime, Model.Attempt, Model.MaxAlarm,
            Model.PathHintsUsed, Mathf.Max(stars, won ? 1 : 0), best, isNewBest);
    }

    public void ExitToMenu()
    {
        if (!HasMenu) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void PollSources()
    {
        if (alarmSources.Count > 0)
        {
            Model.SetAlarm(ReadAlarm());
            if (Model.Alarm >= 1f)
            {
                Lose(LoseReason.MaxAlarm);
                return;
            }
        }

        if (hintSources.Count > 0)
        {
            IInteractionHintSource source = ReadHintSource();
            Model.SetHint(source?.CurrentHint, source?.CurrentHintTarget);
        }
    }

    private float ReadAlarm()
    {
        float max = 0f;
        foreach (IAlarmSource source in alarmSources)
        {
            if (source is Object obj && obj == null) continue;
            max = Mathf.Max(max, source.CurrentAlarm);
        }
        return max;
    }

    private IInteractionHintSource ReadHintSource()
    {
        foreach (IInteractionHintSource source in hintSources)
        {
            if (source is Object obj && obj == null) continue;
            if (!string.IsNullOrEmpty(source.CurrentHint)) return source;
        }
        return null;
    }
}
