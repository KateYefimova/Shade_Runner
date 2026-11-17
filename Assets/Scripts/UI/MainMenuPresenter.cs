using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MainMenuView))]
public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Kate_Map";

    private MainMenuView view;

    private void Awake()
    {
        Time.timeScale = 1f;
        view = GetComponent<MainMenuView>();
    }

    private void OnEnable()
    {
        view.PlayClicked += StartGame;
        view.QuitClicked += QuitGame;
    }

    private void OnDisable()
    {
        view.PlayClicked -= StartGame;
        view.QuitClicked -= QuitGame;
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
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
