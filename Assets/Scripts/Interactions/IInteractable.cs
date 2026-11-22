using UnityEngine;

public interface IInteractable
{
    bool CanInteract(PlayerInteraction player);

    // Предмет видно, але дія недоступна: підказка без [E], натискання ігнорується.
    bool IsLocked(PlayerInteraction player);

    string GetActionHint(PlayerInteraction player);

    Transform HintAnchor { get; }

    void Interact(PlayerInteraction player);
}
