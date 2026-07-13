using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    public GameObject gameOverScreen;
    public GameObject levelClearScreen;
    public GameObject gameUI;

    [SerializeField] string nextSceneName;
    [SerializeField] private TextMeshProUGUI rewardText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void GameOver()
    {
        if (LevelCashManager.Instance != null)
            LevelCashManager.Instance.FailLevelAndLoseCash();

        Time.timeScale = 0f;
        gameUI.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    public void LevelClear()
    {
        int reward = 0;

        if (LevelCashManager.Instance != null)
            reward = LevelCashManager.Instance.CompleteLevelAndGiveReward();

        if (rewardText != null)
            rewardText.text = "$" + reward;

        Time.timeScale = 0f;
        gameUI.SetActive(false);
        levelClearScreen.SetActive(true);
    }

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