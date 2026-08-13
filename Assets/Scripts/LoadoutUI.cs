using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUI : MonoBehaviour
{
    public WeaponSelectionUI selectionUI;

    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopButton;
    [SerializeField] private TextMeshProUGUI shopButtonText;

    [Header("Shop Unlock")]
    [SerializeField] private int requiredUnlockedLevel = 4;

    [Header("Slots")]
    [SerializeField] private LoadoutSlotUI[] slots;

    void OnEnable()
    {
        RefreshShopButton();
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        foreach (LoadoutSlotUI slot in slots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }


    public void OpenPrimary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Primary);
        gameObject.SetActive(false);
    }

    public void OpenSecondary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Secondary);
        gameObject.SetActive(false);
    }

    public void OpenMelee()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Melee);
        gameObject.SetActive(false);
    }

    public void OpenShop()
    {
        if (!IsShopUnlocked())
        {
            Debug.Log("Shop is locked. Beat Level 3 first!");
            return;
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    void RefreshShopButton()
    {
        bool unlocked = IsShopUnlocked();

        if (shopButton != null)
            shopButton.interactable = unlocked;

        if (shopButtonText != null)
        {
            shopButtonText.text = unlocked
                ? "Shop"
                : "Shop Locked";
        }
    }

    bool IsShopUnlocked()
    {
        if (LevelManager.Instance == null)
            return false;

        return LevelManager.Instance.GetUnlockedLevel() >= requiredUnlockedLevel;
    }
}