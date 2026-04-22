using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    public GameObject gameOverScreen;
    public GameObject levelClearScreen;
    public GameObject gameUI;
    [SerializeField] string nextSceneName;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameUI.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    public void LevelClear()
    {
        Time.timeScale = 0f;
        gameUI.SetActive(false);
        levelClearScreen.SetActive(true);
    }

    // ===== BUTTON FUNCTIONS =====

    public void NextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            LevelManager.Instance.LoadLevel(nextSceneName);
        }
    }

    public void Retry()
    {
        LevelManager.Instance.ReloadLevel();
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
