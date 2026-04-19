using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private const string KEY = "UnlockedLevel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KEY, 1);
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int unlocked = GetUnlockedLevel();

        if (currentLevel >= unlocked)
        {
            PlayerPrefs.SetInt(KEY, currentLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f; // ALWAYS reset time before loading
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}