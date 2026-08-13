using System.Collections.Generic;
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

    [SerializeField] private WeaponStatsPanelUI statsPanel;

    private SlotType currentSlot;
    private int currentIndex;

    private List<GunData> currentGunList = new List<GunData>();
    private List<MeleeData> currentMeleeList = new List<MeleeData>();

    public void Open(SlotType slot)
    {
        gameObject.SetActive(true);
        currentSlot = slot;
        currentIndex = 0;

        BuildOwnedWeaponList();

        RefreshUI();
    }

    void BuildOwnedWeaponList()
    {
        currentGunList.Clear();
        currentMeleeList.Clear();

        if (WeaponOwnershipManager.Instance == null)
        {
            Debug.LogWarning("WeaponOwnershipManager not found!");
            return;
        }

        switch (currentSlot)
        {
            case SlotType.Primary:
                foreach (GunData gun in database.primaryGuns)
                {
                    if (WeaponOwnershipManager.Instance.IsGunOwned(gun))
                        currentGunList.Add(gun);
                }
                break;

            case SlotType.Secondary:
                foreach (GunData gun in database.secondaryGuns)
                {
                    if (WeaponOwnershipManager.Instance.IsGunOwned(gun))
                        currentGunList.Add(gun);
                }
                break;

            case SlotType.Melee:
                foreach (MeleeData melee in database.meleeWeapons)
                {
                    if (WeaponOwnershipManager.Instance.IsMeleeOwned(melee))
                        currentMeleeList.Add(melee);
                }
                break;
        }
    }

    public void Next()
    {
        int count = GetCurrentCount();

        if (count <= 0) return;

        currentIndex = (currentIndex + 1) % count;
        RefreshUI();
    }

    public void Previous()
    {
        int count = GetCurrentCount();

        if (count <= 0) return;

        currentIndex = (currentIndex - 1 + count) % count;
        RefreshUI();
    }

    void RefreshUI()
    {
        int count = GetCurrentCount();

        if (count <= 0)
        {
            weaponNameText.text = "No Owned Weapons";

            if (weaponDisplay != null)
                weaponDisplay.sprite = null;

            if (statsPanel != null)
                statsPanel.DisplayStats(null);

            return;
        }

        if (currentSlot == SlotType.Melee)
        {
            MeleeData data = currentMeleeList[currentIndex];

            weaponNameText.text = data.weaponName;

            if (weaponDisplay != null)
                weaponDisplay.sprite = data.icon;

            if (statsPanel != null)
                statsPanel.DisplayStats(data.statRatings);
        }
        else
        {
            GunData data = currentGunList[currentIndex];

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
            return currentMeleeList.Count;

        return currentGunList.Count;
    }

    public void Apply()
    {
        int count = GetCurrentCount();

        if (count <= 0)
        {
            Debug.LogWarning("Cannot apply weapon. No owned weapons available.");
            return;
        }

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