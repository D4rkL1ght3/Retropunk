using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager Instance;

    [Header("Database")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    [Header("Current Loadout")]
    public PlayerLoadout currentLoadout;

    private const string PrimaryGunKey = "Loadout_PrimaryGun";
    private const string SecondaryGunKey = "Loadout_SecondaryGun";
    private const string MeleeWeaponKey = "Loadout_MeleeWeapon";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLoadout();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveLoadout()
    {
        if (currentLoadout == null)
            return;

        if (currentLoadout.primaryGun != null)
            PlayerPrefs.SetString(PrimaryGunKey, currentLoadout.primaryGun.gunName);

        if (currentLoadout.secondaryGun != null)
            PlayerPrefs.SetString(SecondaryGunKey, currentLoadout.secondaryGun.gunName);

        if (currentLoadout.meleeWeapon != null)
            PlayerPrefs.SetString(MeleeWeaponKey, currentLoadout.meleeWeapon.weaponName);

        PlayerPrefs.Save();

        Debug.Log("Loadout saved!");
    }

    public void LoadLoadout()
    {
        if (currentLoadout == null)
            currentLoadout = new PlayerLoadout();

        if (weaponDatabase == null)
        {
            Debug.LogWarning("LoadoutManager has no WeaponDatabase assigned!");
            return;
        }

        string savedPrimary = PlayerPrefs.GetString(PrimaryGunKey, "");
        string savedSecondary = PlayerPrefs.GetString(SecondaryGunKey, "");
        string savedMelee = PlayerPrefs.GetString(MeleeWeaponKey, "");

        if (!string.IsNullOrEmpty(savedPrimary))
        {
            GunData gun = FindGunByName(weaponDatabase.primaryGuns, savedPrimary);

            if (gun != null)
                currentLoadout.primaryGun = gun;
        }

        if (!string.IsNullOrEmpty(savedSecondary))
        {
            GunData gun = FindGunByName(weaponDatabase.secondaryGuns, savedSecondary);

            if (gun != null)
                currentLoadout.secondaryGun = gun;
        }

        if (!string.IsNullOrEmpty(savedMelee))
        {
            MeleeData melee = FindMeleeByName(weaponDatabase.meleeWeapons, savedMelee);

            if (melee != null)
                currentLoadout.meleeWeapon = melee;
        }

        Debug.Log("Loadout loaded!");
    }

    private GunData FindGunByName(GunData[] guns, string gunName)
    {
        foreach (GunData gun in guns)
        {
            if (gun != null && gun.gunName == gunName)
                return gun;
        }

        Debug.LogWarning("Could not find saved gun: " + gunName);
        return null;
    }

    private MeleeData FindMeleeByName(MeleeData[] meleeWeapons, string weaponName)
    {
        foreach (MeleeData melee in meleeWeapons)
        {
            if (melee != null && melee.weaponName == weaponName)
                return melee;
        }

        Debug.LogWarning("Could not find saved melee weapon: " + weaponName);
        return null;
    }

    public void ClearSavedLoadout()
    {
        PlayerPrefs.DeleteKey(PrimaryGunKey);
        PlayerPrefs.DeleteKey(SecondaryGunKey);
        PlayerPrefs.DeleteKey(MeleeWeaponKey);
        PlayerPrefs.Save();

        Debug.Log("Saved loadout cleared!");
    }
}