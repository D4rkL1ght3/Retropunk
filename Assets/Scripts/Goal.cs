using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] int currentLevelIndex;

    [Header("UI")]
    [SerializeField] GameObject levelCompleteUI;

    [Header("Settings")]
    [SerializeField] private bool isLocked = false;

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (completed) return;
        if (isLocked)
        {
            Debug.Log("Goal is locked! Complete required objectives to unlock.");
            return;
        }

        if (collision.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    public void UnlockGoal()
    {
        isLocked = false;
        Debug.Log("Goal Unlocked!");
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

        // Show Level Complete UI
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.LevelClear();
        }
    }
}