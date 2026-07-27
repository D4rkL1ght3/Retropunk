using UnityEngine;

public class WeaponOwnershipManager : MonoBehaviour
{
    public static WeaponOwnershipManager Instance;

    [Header("Database")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    private const string GunOwnedPrefix = "Owned_Gun_";
    private const string MeleeOwnedPrefix = "Owned_Melee_";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultWeapons();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaultWeapons()
    {
        if (weaponDatabase == null)
        {
            Debug.LogWarning("WeaponOwnershipManager has no WeaponDatabase assigned!");
            return;
        }

        foreach (GunData gun in weaponDatabase.primaryGuns)
        {
            if (gun != null && gun.unlockedByDefault)
                UnlockGun(gun);
        }

        foreach (GunData gun in weaponDatabase.secondaryGuns)
        {
            if (gun != null && gun.unlockedByDefault)
                UnlockGun(gun);
        }

        foreach (MeleeData melee in weaponDatabase.meleeWeapons)
        {
            if (melee != null && melee.unlockedByDefault)
                UnlockMelee(melee);
        }
    }

    public bool IsGunOwned(GunData gun)
    {
        if (gun == null) return false;

        if (gun.unlockedByDefault)
            return true;

        return PlayerPrefs.GetInt(GetGunKey(gun), 0) == 1;
    }

    public bool IsMeleeOwned(MeleeData melee)
    {
        if (melee == null) return false;

        if (melee.unlockedByDefault)
            return true;

        return PlayerPrefs.GetInt(GetMeleeKey(melee), 0) == 1;
    }

    public void UnlockGun(GunData gun)
    {
        if (gun == null) return;

        PlayerPrefs.SetInt(GetGunKey(gun), 1);
        PlayerPrefs.Save();

        Debug.Log("Unlocked gun: " + gun.gunName);
    }

    public void UnlockMelee(MeleeData melee)
    {
        if (melee == null) return;

        PlayerPrefs.SetInt(GetMeleeKey(melee), 1);
        PlayerPrefs.Save();

        Debug.Log("Unlocked melee weapon: " + melee.weaponName);
    }

    public bool BuyGun(GunData gun)
    {
        if (gun == null) return false;

        if (IsGunOwned(gun))
        {
            Debug.Log("Gun already owned: " + gun.gunName);
            return false;
        }

        if (CashWallet.Instance == null)
        {
            Debug.LogWarning("CashWallet instance not found!");
            return false;
        }

        if (!CashWallet.Instance.SpendCash(gun.price))
        {
            Debug.Log("Not enough cash to buy: " + gun.gunName);
            return false;
        }

        UnlockGun(gun);
        return true;
    }

    public bool BuyMelee(MeleeData melee)
    {
        if (melee == null) return false;

        if (IsMeleeOwned(melee))
        {
            Debug.Log("Melee weapon already owned: " + melee.weaponName);
            return false;
        }

        if (CashWallet.Instance == null)
        {
            Debug.LogWarning("CashWallet instance not found!");
            return false;
        }

        if (!CashWallet.Instance.SpendCash(melee.price))
        {
            Debug.Log("Not enough cash to buy: " + melee.weaponName);
            return false;
        }

        UnlockMelee(melee);
        return true;
    }

    private string GetGunKey(GunData gun)
    {
        return GunOwnedPrefix + gun.gunName;
    }

    private string GetMeleeKey(MeleeData melee)
    {
        return MeleeOwnedPrefix + melee.weaponName;
    }
}