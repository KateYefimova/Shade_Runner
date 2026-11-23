using UnityEngine;

public interface IInteractable
{
    bool CanInteract(PlayerInteraction player);

    // Предмет видно, але дія недоступна: підказка показується без [E].
    bool IsLocked(PlayerInteraction player);

    string GetActionHint(PlayerInteraction player);

    Transform HintAnchor { get; }

    void Interact(PlayerInteraction player);
}
