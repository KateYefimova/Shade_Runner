using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class UiSelection
{
    public static void Focus(Selectable target)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || target == null) return;

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(target.gameObject);
    }

    public static void Clear()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public static void KeepFocus(Selectable fallback)
    {
        EventSystem eventSystem = EventSystem.current;
        Keyboard keyboard = Keyboard.current;
        if (eventSystem == null || keyboard == null || fallback == null) return;

        GameObject current = eventSystem.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy) return;

        bool navigating = keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
                          keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame ||
                          keyboard.tabKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
        if (navigating) Focus(fallback);
    }
}
