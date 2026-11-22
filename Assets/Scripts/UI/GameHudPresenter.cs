using System;

public class GameHudPresenter : IDisposable
{
    private readonly GameSessionModel model;
    private readonly GameHudView view;
    private readonly GameManager game;

    public GameHudPresenter(GameSessionModel model, GameHudView view, GameManager game)
    {
        this.model = model;
        this.view = view;
        this.game = game;

        model.StateChanged += OnStateChanged;
        model.AlarmChanged += OnAlarmChanged;
        model.HintChanged += OnHintChanged;
        model.AttemptStarted += OnAttemptStarted;

        view.PauseClicked += game.Pause;
        view.ResumeClicked += game.Resume;
        view.RestartClicked += game.Restart;
        view.MenuClicked += game.ExitToMenu;

        view.SetMenuAvailable(game.HasMenu);
        view.SetAlarm(model.Alarm, true);
        OnStateChanged(model.State);
    }

    public void Dispose()
    {
        model.StateChanged -= OnStateChanged;
        model.AlarmChanged -= OnAlarmChanged;
        model.HintChanged -= OnHintChanged;
        model.AttemptStarted -= OnAttemptStarted;

        view.PauseClicked -= game.Pause;
        view.ResumeClicked -= game.Resume;
        view.RestartClicked -= game.Restart;
        view.MenuClicked -= game.ExitToMenu;
    }

    private void OnStateChanged(GameState state)
    {
        view.ShowState(state, DefeatText(model.LoseReason));
        view.SetHint(state == GameState.Playing ? model.InteractionHint : string.Empty, model.HintTarget);
    }

    private void OnAlarmChanged(float value) => view.SetAlarm(value);

    private void OnHintChanged(string hint)
    {
        if (model.IsPlaying) view.SetHint(hint, model.HintTarget);
    }

    private void OnAttemptStarted() => view.SetAlarm(model.Alarm, true);

    private static string DefeatText(LoseReason reason) => reason switch
    {
        LoseReason.Caught => "Охоронець схопив вас",
        LoseReason.MaxAlarm => "Тривога досягла максимуму",
        _ => string.Empty
    };
}
