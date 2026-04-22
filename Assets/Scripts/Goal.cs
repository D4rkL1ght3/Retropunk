using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("Level Info")]
    [SerializeField] int currentLevelIndex;

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

        // Show Level Complete UI
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.LevelClear();
        }
    }
}