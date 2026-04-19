using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int levelNumber;
    public Button button;
    public GameObject lockIcon;
    public GameObject levelText;

    private void Start()
    {
        int unlockedLevel = LevelManager.Instance.GetUnlockedLevel();

        if (levelNumber <= unlockedLevel)
        {
            button.interactable = true;
            lockIcon.SetActive(false);
            levelText.SetActive(true);
        }
        else
        {
            button.interactable = false;
            lockIcon.SetActive(true);
            levelText.SetActive(false);
        }
    }

    public void LoadLevel(string sceneName)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(sceneName);
        }
        else
        {
            Debug.LogError("LevelManager instance not found!");
        }
    }
}