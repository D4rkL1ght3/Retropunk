using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject levelSelect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenLevelSelect()
    {
        titleScreen.SetActive(false);
        levelSelect.SetActive(true);
    }

    public void BackToMainMenu()
    {
        levelSelect.SetActive(false);
        titleScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}