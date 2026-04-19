using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject levelSelect;

    public void OpenLevelSelect()
    {
        levelSelect.SetActive(true);
        titleScreen.SetActive(false);
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