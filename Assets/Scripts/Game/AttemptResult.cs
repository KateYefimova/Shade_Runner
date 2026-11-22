public readonly struct AttemptResult
{
    public readonly bool Won;
    public readonly float Time;
    public readonly int Attempt;
    public readonly float MaxAlarm;
    public readonly int PathHints;
    public readonly int Stars;
    public readonly float BestTime;
    public readonly bool IsNewBest;

    public AttemptResult(bool won, float time, int attempt, float maxAlarm, int pathHints, int stars, float bestTime, bool isNewBest)
    {
        Won = won;
        Time = time;
        Attempt = attempt;
        MaxAlarm = maxAlarm;
        PathHints = pathHints;
        Stars = stars;
        BestTime = bestTime;
        IsNewBest = isNewBest;
    }
}
