using TMPro;
using UnityEngine;

public class LevelCashManager : MonoBehaviour
{
    public static LevelCashManager Instance;

    [Header("Level Cash Settings")]
    [SerializeField] private int levelID = 1;
    [SerializeField] private int firstClearPaycheck = 50;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelCashText;

    public int CurrentLevelCash { get; private set; }

    void Awake()
    {
        Instance = this;
        CurrentLevelCash = 0;
        UpdateCashUI();
    }

    public void CollectCash(int amount)
    {
        if (amount <= 0) return;

        CurrentLevelCash += amount;
        UpdateCashUI();

        Debug.Log("Collected $" + amount + ". Level cash: $" + CurrentLevelCash);
    }

    public int CompleteLevelAndGiveReward()
    {
        int totalReward = CurrentLevelCash;

        bool firstClear = !HasBeatenLevel();

        if (firstClear)
        {
            totalReward += firstClearPaycheck;
            MarkLevelAsBeaten();

            Debug.Log("First clear paycheck awarded: $" + firstClearPaycheck);
        }

        if (CashWallet.Instance != null)
        {
            CashWallet.Instance.AddCash(totalReward);
        }
        else
        {
            Debug.LogWarning("CashWallet instance not found!");
        }

        Debug.Log("Level reward given: $" + totalReward);

        CurrentLevelCash = 0;
        UpdateCashUI();

        return totalReward;
    }

    public void FailLevelAndLoseCash()
    {
        Debug.Log("Player died. Lost temporary level cash: $" + CurrentLevelCash);

        CurrentLevelCash = 0;
        UpdateCashUI();
    }

    void UpdateCashUI()
    {
        if (levelCashText != null)
        {
            levelCashText.text = "$" + CurrentLevelCash;
        }
    }

    public bool HasBeatenLevel()
    {
        return PlayerPrefs.GetInt(GetLevelBeatenKey(), 0) == 1;
    }

    private void MarkLevelAsBeaten()
    {
        PlayerPrefs.SetInt(GetLevelBeatenKey(), 1);
        PlayerPrefs.Save();
    }

    private string GetLevelBeatenKey()
    {
        return "Level_Beaten_" + levelID;
    }

    public int GetFirstClearPaycheck()
    {
        return firstClearPaycheck;
    }

    public int GetLevelID()
    {
        return levelID;
    }
}