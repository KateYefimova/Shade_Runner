using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum VolumeChannel
{
    Master,
    Music,
    Sfx,
    Ui
}

public class GameAudio : MonoBehaviour
{
    private const string MixerPath = "Audio/MainMixer";
    private const float DefaultVolume = 0.8f;

    public static GameAudio Instance { get; private set; }

    public AudioMixer Mixer { get; private set; }

    private AudioSource uiSource;
    private AudioSource resultSource;
    private AudioClip clickClip;
    private AudioClip winClip;
    private AudioClip loseClip;
    private GameSessionModel observedModel;
    private AudioListener fallbackListener;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject(nameof(GameAudio));
        DontDestroyOnLoad(go);
        go.AddComponent<GameAudio>();
    }

    private void Awake()
    {
        Instance = this;

        Mixer = Resources.Load<AudioMixer>(MixerPath);
        clickClip = Resources.Load<AudioClip>("Audio/ui_click");
        winClip = Resources.Load<AudioClip>("Audio/win");
        loseClip = Resources.Load<AudioClip>("Audio/lose");

        fallbackListener = gameObject.AddComponent<AudioListener>();
        fallbackListener.enabled = false;

        uiSource = CreateSource("UI");
        resultSource = CreateSource("SFX");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        foreach (VolumeChannel channel in Enum.GetValues(typeof(VolumeChannel)))
            ApplyVolume(channel, GetVolume(channel));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Observe(null);
    }

    public AudioMixerGroup FindGroup(string groupName)
    {
        if (Mixer == null) return null;

        foreach (AudioMixerGroup group in Mixer.FindMatchingGroups(string.Empty))
            if (group.name == groupName) return group;

        return null;
    }

    public float GetVolume(VolumeChannel channel)
    {
        return PlayerPrefs.GetFloat(VolumeKey(channel), DefaultVolume);
    }

    public void SetVolume(VolumeChannel channel, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumeKey(channel), value);
        ApplyVolume(channel, value);
    }

    public void PlayClick()
    {
        if (clickClip != null) uiSource.PlayOneShot(clickClip);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioListener.pause = false;
        UpdateFallbackListener();

        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            button.onClick.AddListener(PlayClick);

        Observe(GameManager.Instance != null ? GameManager.Instance.Model : null);
    }

    private void UpdateFallbackListener()
    {
        bool sceneHasListener = false;
        foreach (AudioListener listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
        {
            if (listener != fallbackListener && listener.enabled)
                sceneHasListener = true;
        }
        fallbackListener.enabled = !sceneHasListener;
    }

    private void Observe(GameSessionModel model)
    {
        if (observedModel != null) observedModel.StateChanged -= OnStateChanged;
        observedModel = model;
        if (observedModel != null) observedModel.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        AudioListener.pause = state != GameState.Playing;

        if (state == GameState.Won && winClip != null) resultSource.PlayOneShot(winClip);
        if (state == GameState.Lost && loseClip != null) resultSource.PlayOneShot(loseClip);
    }

    private AudioSource CreateSource(string groupName)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.ignoreListenerPause = true;
        source.outputAudioMixerGroup = FindGroup(groupName);
        return source;
    }

    private void ApplyVolume(VolumeChannel channel, float value)
    {
        if (Mixer == null) return;

        float decibels = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        Mixer.SetFloat(ParameterName(channel), decibels);
    }

    private static string ParameterName(VolumeChannel channel) => channel switch
    {
        VolumeChannel.Sfx => "SFXVolume",
        VolumeChannel.Ui => "UIVolume",
        _ => channel + "Volume"
    };

    private static string VolumeKey(VolumeChannel channel) => "volume_" + channel;
}
