using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject loadoutSelection;

    public void OpenLevelSelect()
    {
        levelSelect.SetActive(true);
        titleScreen.SetActive(false);
    }

    public void BackToMainMenu()
    {
        levelSelect.SetActive(false);
        loadoutSelection.SetActive(false);
        titleScreen.SetActive(true);
    }

    public void OpenLoadoutSelection()
    {
        loadoutSelection.SetActive(true);
        titleScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}