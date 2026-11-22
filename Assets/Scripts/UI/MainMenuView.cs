using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    public event Action PlayClicked;
    public event Action QuitClicked;

    private void Awake()
    {
        playButton.onClick.AddListener(() => PlayClicked?.Invoke());
        quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
    }
}
