using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour, IEntity
{
    public void Disable()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }
    public void OnDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
