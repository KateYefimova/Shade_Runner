using UnityEngine;

public class AlertTimerHUD : MonoBehaviour
{
    private GuardVision[] guards;

    private void Start()
    {
        guards = FindObjectsByType<GuardVision>(FindObjectsSortMode.None);
    }

    private void OnGUI()
    {
        if (guards == null) return;

        float minLeft = float.MaxValue;
        float total = 5f;
        bool anyDetected = false;

        foreach (GuardVision g in guards)
        {
            if (g == null || !g.IsPlayerDetected) continue;

            anyDetected = true;
            if (g.AlertTimeLeft < minLeft)
            {
                minLeft = g.AlertTimeLeft;
                total = g.TimeToEscape;
            }
        }

        if (!anyDetected) return;

        Rect bar = new Rect(Screen.width / 2f - 200f, 20f, 400f, 34f);

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);

        GUI.color = Color.red;
        GUI.DrawTexture(
            new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(minLeft / total), bar.height),
            Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        GUI.Label(bar, "ТРИВОГА! Вийдіть зі світла: " + minLeft.ToString("0.0") + " с", style);
    }
}