using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MainMenuView))]
public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Yelizaveta_Map";

    private MainMenuView view;
    private bool settingsOpen;

    private void Awake()
    {
        Time.timeScale = 1f;
        view = GetComponent<MainMenuView>();
    }

    private void OnEnable()
    {
        view.PlayClicked += StartGame;
        view.SettingsClicked += OpenSettings;
        view.BackClicked += CloseSettings;
        view.QuitClicked += QuitGame;
    }

    private void OnDisable()
    {
        view.PlayClicked -= StartGame;
        view.SettingsClicked -= OpenSettings;
        view.BackClicked -= CloseSettings;
        view.QuitClicked -= QuitGame;
    }

    private void Update()
    {
        if (settingsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseSettings();
    }

    private void StartGame()
    {
        ScreenFader.Run(() => SceneManager.LoadScene(gameSceneName));
    }

    private void OpenSettings()
    {
        settingsOpen = true;
        view.ShowSettings(true);
    }

    private void CloseSettings()
    {
        settingsOpen = false;
        view.ShowSettings(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
