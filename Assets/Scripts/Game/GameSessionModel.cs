using System;
using UnityEngine;

public class GameSessionModel
{
    public GameState State { get; private set; } = GameState.Playing;
    public LoseReason LoseReason { get; private set; } = LoseReason.None;
    public float Alarm { get; private set; }
    public string InteractionHint { get; private set; } = string.Empty;

    public bool IsPlaying => State == GameState.Playing;
    public bool IsFinished => State == GameState.Won || State == GameState.Lost;

    public event Action<GameState> StateChanged;
    public event Action<float> AlarmChanged;
    public event Action<string> HintChanged;
    public event Action AttemptStarted;

    public void SetState(GameState newState, LoseReason reason = LoseReason.None)
    {
        if (State == newState) return;

        State = newState;
        LoseReason = newState == GameState.Lost ? reason : LoseReason.None;
        StateChanged?.Invoke(State);
    }

    public void SetAlarm(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(Alarm, value)) return;

        Alarm = value;
        AlarmChanged?.Invoke(Alarm);
    }

    public void SetHint(string hint)
    {
        hint ??= string.Empty;
        if (InteractionHint == hint) return;

        InteractionHint = hint;
        HintChanged?.Invoke(InteractionHint);
    }

    public void ResetForNewAttempt()
    {
        SetAlarm(0f);
        SetHint(string.Empty);
        SetState(GameState.Playing);
        AttemptStarted?.Invoke();
    }
}
