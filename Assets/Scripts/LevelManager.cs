using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private const string KEY = "UnlockedLevel";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KEY, 1); // default = Level 1
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
        SceneManager.LoadScene(sceneName);
    }
}