using UnityEngine;
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
    public TextMeshProUGUI statsText;

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

            statsText.text =
                $"Damage: {data.damage}\n" +
                $"Range: {data.range}\n" +
                $"Cooldown: {data.cooldown}";
        }
        else
        {
            var data = currentGunList[currentIndex];

            weaponNameText.text = data.gunName;

            statsText.text =
                $"Damage: {data.damage}\n" +
                $"Fire Rate: {data.fireRate}\n" +
                $"Ammo: {data.maxAmmo}\n" +
                $"Reload: {data.reloadTime}";
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

        gameObject.SetActive(false);
    }
}