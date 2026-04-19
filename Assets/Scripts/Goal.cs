using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] int currentLevelIndex;
    [SerializeField] string nextSceneName;

    [Header("UI")]
    [SerializeField] GameObject levelCompleteUI;

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (completed) return;

        if (collision.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        completed = true;

        Debug.Log("LEVEL COMPLETE!");

        // Unlock next level
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnlockNextLevel(currentLevelIndex);
        }

        // Show UI
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }

        // Pause game
        Time.timeScale = 0f;
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