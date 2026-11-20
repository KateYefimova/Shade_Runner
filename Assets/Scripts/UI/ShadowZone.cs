using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ShadowZone : MonoBehaviour
{
    private BoxCollider2D zoneCollider;
    private PlayerStatus playerInZone;
    private readonly HashSet<Collider2D> playerColliders = new();

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStatus player = other.GetComponentInParent<PlayerStatus>();

        if (player == null)
            return;

        playerInZone = player;
        playerColliders.Add(other);

        player.SetInShadow(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playerColliders.Remove(other))
            return;

        if (playerColliders.Count == 0)
        {
            if (playerInZone != null)
                playerInZone.SetInShadow(false);

            playerInZone = null;
        }
    }

    private void OnDisable()
    {
        if (playerInZone != null)
            playerInZone.SetInShadow(false);

        playerColliders.Clear();
        playerInZone = null;
    }
}