using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class StealthLighting : MonoBehaviour
{
    [SerializeField] private float ambientIntensity = 0.22f;
    [SerializeField] private Color ambientColor = new Color(0.45f, 0.55f, 0.85f);

    [SerializeField] private Color flashlightColor = new Color(1f, 0.88f, 0.55f);
    [SerializeField] private float flashlightIntensity = 1.3f;

    [SerializeField] private float propRadius = 2.2f;
    [SerializeField] private float propIntensity = 0.9f;

    [SerializeField] private Color exitColor = new Color(0.3f, 1f, 0.6f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (FindFirstObjectByType<GameManager>() != null && FindFirstObjectByType<StealthLighting>() == null)
                new GameObject(nameof(StealthLighting)).AddComponent<StealthLighting>();
        };
    }

    private void Start()
    {
        DimGlobalLights();
        AddGuardFlashlights();
        AddPropLights();
        AddExitLight();
    }

    private void DimGlobalLights()
    {
        foreach (Light2D light in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (light.lightType != Light2D.LightType.Global) continue;

            light.intensity = ambientIntensity;
            light.color = ambientColor;
        }
    }

    private void AddGuardFlashlights()
    {
        foreach (GuardVision vision in FindObjectsByType<GuardVision>(FindObjectsSortMode.None))
        {
            GuardAI ai = vision.GetComponent<GuardAI>();
            if (ai == null || vision.GetComponentInChildren<GuardFlashlight>() != null) continue;

            Light2D light = CreateLight("Flashlight", vision.transform, flashlightColor, flashlightIntensity);
            light.pointLightOuterRadius = vision.ViewDistance;
            light.pointLightInnerRadius = vision.ViewDistance * 0.3f;
            light.pointLightOuterAngle = vision.ViewAngle;
            light.pointLightInnerAngle = vision.ViewAngle * 0.6f;
            light.gameObject.AddComponent<GuardFlashlight>().Init(ai);
        }
    }

    private void AddPropLights()
    {
        foreach (PropAnimation prop in FindObjectsByType<PropAnimation>(FindObjectsSortMode.None))
        {
            Light2D light = CreateLight("PropLight", prop.transform, PropColor(prop.name), propIntensity);
            light.pointLightOuterRadius = propRadius;
            light.pointLightInnerRadius = propRadius * 0.2f;
        }
    }

    private void AddExitLight()
    {
        foreach (ExitZone exit in FindObjectsByType<ExitZone>(FindObjectsSortMode.None))
        {
            Light2D light = CreateLight("ExitLight", exit.transform, exitColor, 1f);
            Bounds bounds = exit.GetComponent<Collider2D>().bounds;
            light.transform.position = bounds.center;
            light.pointLightOuterRadius = Mathf.Max(bounds.extents.x, bounds.extents.y) + 1.5f;
            light.pointLightInnerRadius = 0.3f;
        }
    }

    private static Light2D CreateLight(string objectName, Transform parent, Color color, float intensity)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.transform.localScale = new Vector3(
            1f / Mathf.Max(0.0001f, Mathf.Abs(parent.lossyScale.x)),
            1f / Mathf.Max(0.0001f, Mathf.Abs(parent.lossyScale.y)),
            1f);

        Light2D light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.falloffIntensity = 0.6f;
        light.pointLightOuterAngle = 360f;
        light.pointLightInnerAngle = 360f;
        light.shadowsEnabled = true;
        light.shadowIntensity = 0.9f;
        return light;
    }

    private static Color PropColor(string propName)
    {
        string lower = propName.ToLowerInvariant();
        if (lower.Contains("warning") || lower.Contains("beacon")) return new Color(1f, 0.35f, 0.25f);
        if (lower.Contains("reactor")) return new Color(0.4f, 1f, 0.55f);
        if (lower.Contains("server")) return new Color(0.35f, 0.55f, 1f);
        if (lower.Contains("vent")) return new Color(0.6f, 0.7f, 0.8f);
        return new Color(0.2f, 0.9f, 1f);
    }
}
