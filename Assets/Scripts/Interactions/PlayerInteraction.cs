using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float interactionDistance = 1.5f;

    private Rigidbody2D playerRb;
    private DraggableBox heldBox;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        // E releases the currently held box.
        if (heldBox != null)
        {
            heldBox.Release();
            heldBox = null;
            return;
        }

        DoorInteractable nearestDoor = null;
        DraggableBox nearestBox = null;

        float minDistance = interactionDistance;

        foreach (DoorInteractable door in
                 FindObjectsByType<DoorInteractable>(
                     FindObjectsSortMode.None))
        {
            float distance = Vector2.Distance(
                transform.position,
                door.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDoor = door;
                nearestBox = null;
            }
        }

        foreach (DraggableBox box in
                 FindObjectsByType<DraggableBox>(
                     FindObjectsSortMode.None))
        {
            if (box.IsGrabbed)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                box.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestBox = box;
                nearestDoor = null;
            }
        }

        if (nearestDoor != null)
        {
            nearestDoor.Interact();
        }
        else if (nearestBox != null)
        {
            nearestBox.Grab(playerRb);
            heldBox = nearestBox;
        }
    }
}