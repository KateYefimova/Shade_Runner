public interface IAlarmSource
{
    float CurrentAlarm { get; }
}

public interface IInteractionHintSource
{
    string CurrentHint { get; }
}
