using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Database")]
public class WeaponDatabase : ScriptableObject
{
    public GunData[] primaryGuns;
    public GunData[] secondaryGuns;
    public MeleeData[] meleeWeapons;
}