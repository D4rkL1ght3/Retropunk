using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAnimationEvents : MonoBehaviour
{
    public void Attack()
    {
        transform.root.SendMessage("DealMeleeDamage", SendMessageOptions.DontRequireReceiver);
    }

    public void OnDeath()
    {
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.GameOver();
        }
    }
}
