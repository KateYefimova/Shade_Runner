using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels (вимкнені на старті)")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Next level")]
    [Tooltip("порожній, якщо наступного рівня немає")]
    [SerializeField] private string nextSceneName;

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Restart();
    }

    public void Lose()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Win()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}