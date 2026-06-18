using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSelectionUI : MonoBehaviour
{
    public enum SlotType
    {
        Primary,
        Secondary,
        Melee
    }

    public WeaponDatabase database;

    [Header("UI")]
    public TextMeshProUGUI weaponNameText;
    public Image weaponDisplay;
    public GameObject loadoutSelectPanel;
    public WeaponStatsPanelUI statsPanel;

    private SlotType currentSlot;
    private int currentIndex;

    private GunData[] currentGunList;
    private MeleeData[] currentMeleeList;

    public void Open(SlotType slot)
    {
        gameObject.SetActive(true);
        currentSlot = slot;
        currentIndex = 0;

        switch (slot)
        {
            case SlotType.Primary:
                currentGunList = database.primaryGuns;
                break;
            case SlotType.Secondary:
                currentGunList = database.secondaryGuns;
                break;
            case SlotType.Melee:
                currentMeleeList = database.meleeWeapons;
                break;
        }

        RefreshUI();
    }

    public void Next()
    {
        int count = GetCurrentCount();
        currentIndex = (currentIndex + 1) % count;
        RefreshUI();
    }

    public void Previous()
    {
        int count = GetCurrentCount();
        currentIndex = (currentIndex - 1 + count) % count;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (currentSlot == SlotType.Melee)
        {
            var data = currentMeleeList[currentIndex];

            weaponNameText.text = data.weaponName;

            if (weaponDisplay != null)
                weaponDisplay.sprite = data.icon;

            if (statsPanel != null)
                statsPanel.DisplayStats(data.statRatings);
        }
        else
        {
            var data = currentGunList[currentIndex];

            weaponNameText.text = data.gunName;

            if (weaponDisplay != null)
                weaponDisplay.sprite = data.baseSprite;

            if (statsPanel != null)
                statsPanel.DisplayStats(data.statRatings);
        }
    }

    int GetCurrentCount()
    {
        if (currentSlot == SlotType.Melee)
            return currentMeleeList.Length;

        return currentGunList.Length;
    }

    public void Apply()
    {
        var loadout = LoadoutManager.Instance.currentLoadout;

        switch (currentSlot)
        {
            case SlotType.Primary:
                loadout.primaryGun = currentGunList[currentIndex];
                break;

            case SlotType.Secondary:
                loadout.secondaryGun = currentGunList[currentIndex];
                break;

            case SlotType.Melee:
                loadout.meleeWeapon = currentMeleeList[currentIndex];
                break;
        }

        LoadoutManager.Instance.SaveLoadout();

        gameObject.SetActive(false);
        loadoutSelectPanel.SetActive(true);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
        loadoutSelectPanel.SetActive(true);
    }
}