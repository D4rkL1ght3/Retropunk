using UnityEngine;

public class CashWallet : MonoBehaviour
{
    public static CashWallet Instance;

    private const string CASH_KEY = "Player_Cash";

    public int CurrentCash { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCash();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadCash()
    {
        CurrentCash = PlayerPrefs.GetInt(CASH_KEY, 0);
    }

    public void AddCash(int amount)
    {
        if (amount <= 0) return;

        CurrentCash += amount;
        SaveCash();

        Debug.Log("Added cash: " + amount + ". Total cash: " + CurrentCash);
    }

    public bool SpendCash(int amount)
    {
        if (amount <= 0) return true;

        if (CurrentCash < amount)
        {
            Debug.Log("Not enough cash!");
            return false;
        }

        CurrentCash -= amount;
        SaveCash();

        Debug.Log("Spent cash: " + amount + ". Total cash: " + CurrentCash);
        return true;
    }

    public bool CanAfford(int amount)
    {
        return CurrentCash >= amount;
    }

    void SaveCash()
    {
        PlayerPrefs.SetInt(CASH_KEY, CurrentCash);
        PlayerPrefs.Save();
    }

    public void ResetCash()
    {
        CurrentCash = 0;
        SaveCash();
    }
}