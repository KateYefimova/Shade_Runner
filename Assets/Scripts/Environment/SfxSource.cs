using UnityEngine;

public static class SfxSource
{
    public static AudioSource Create(GameObject owner)
    {
        AudioSource source = owner.AddComponent<AudioSource>();
        source.playOnAwake = false;

        if (GameAudio.Instance != null)
            source.outputAudioMixerGroup = GameAudio.Instance.FindGroup("SFX");

        return source;
    }
}
