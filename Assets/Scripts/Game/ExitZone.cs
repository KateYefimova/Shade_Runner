using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitZone : MonoBehaviour
{
    private void Awake()
    {
        if (GetComponent<ExitGuide>() == null) gameObject.AddComponent<ExitGuide>();
        if (GetComponent<ExitPathGuide>() == null) gameObject.AddComponent<ExitPathGuide>();
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStatus player = other.GetComponentInParent<PlayerStatus>();

        if (player != null)
            player.Win();
    }
}
