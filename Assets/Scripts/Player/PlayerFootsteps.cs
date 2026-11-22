using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private float walkInterval = 0.38f;
    [SerializeField] private float sneakInterval = 0.62f;
    [SerializeField] private float carryInterval = 0.52f;
    [SerializeField] private float walkVolume = 0.6f;
    [SerializeField] private float sneakVolume = 0.2f;

    private PlayerMovement movement;
    private PlayerInteraction interaction;
    private AudioSource source;
    private AudioClip[] clips;
    private float timer;
    private int lastClip = -1;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();
        clips = Resources.LoadAll<AudioClip>("Audio/Steps");

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void Start()
    {
        if (GameAudio.Instance != null)
            source.outputAudioMixerGroup = GameAudio.Instance.FindGroup("SFX");
    }

    private void Update()
    {
        if (clips.Length == 0 || !movement.IsMoving || !GameManager.IsGameplayActive)
        {
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        bool carrying = interaction != null && interaction.IsHoldingBox;
        bool sneaking = movement.IsSneaking;
        timer = carrying ? carryInterval : sneaking ? sneakInterval : walkInterval;

        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1 && index == lastClip) index = (index + 1) % clips.Length;
        lastClip = index;

        source.pitch = Random.Range(0.92f, 1.08f) * (carrying ? 0.9f : 1f);
        source.PlayOneShot(clips[index], sneaking ? sneakVolume : walkVolume);
    }
}
