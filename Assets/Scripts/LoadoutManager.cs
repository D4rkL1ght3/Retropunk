using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager Instance;

    public PlayerLoadout currentLoadout;

    void Awake()
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
}