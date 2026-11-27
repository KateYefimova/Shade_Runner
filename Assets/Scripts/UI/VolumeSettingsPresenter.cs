using System;
using UnityEngine;

[RequireComponent(typeof(VolumeSettingsView))]
public class VolumeSettingsPresenter : MonoBehaviour
{
    private VolumeSettingsView view;

    private void Start()
    {
        view = GetComponent<VolumeSettingsView>();
        if (GameAudio.Instance == null) return;

        foreach (VolumeChannel channel in Enum.GetValues(typeof(VolumeChannel)))
            view.SetValue(channel, GameAudio.Instance.GetVolume(channel));

        view.VolumeChanged += OnVolumeChanged;
    }

    private void OnDestroy()
    {
        if (view != null) view.VolumeChanged -= OnVolumeChanged;
    }

    private static void OnVolumeChanged(VolumeChannel channel, float value)
    {
        GameAudio.Instance.SetVolume(channel, value);
    }
}
