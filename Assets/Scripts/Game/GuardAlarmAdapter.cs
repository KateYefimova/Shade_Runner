using UnityEngine;

public class GuardAlarmAdapter : MonoBehaviour, IAlarmSource
{
    private GuardVision[] guards;

    private void Awake()
    {
        guards = FindObjectsByType<GuardVision>(FindObjectsSortMode.None);
    }

    public float CurrentAlarm
    {
        get
        {
            float max = 0f;
            foreach (GuardVision guard in guards)
            {
                if (guard == null || guard.TimeToEscape <= 0f) continue;
                max = Mathf.Max(max, 1f - guard.AlertTimeLeft / guard.TimeToEscape);
            }
            return Mathf.Clamp01(max);
        }
    }
}
