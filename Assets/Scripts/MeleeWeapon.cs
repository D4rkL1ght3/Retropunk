using UnityEngine;

[System.Serializable]
public class MeleeWeapon
{
    public string weaponName;

    [Header("Stats")]
    public int damage = 10;
    public float range = 0.5f;
    public float cooldown = 0.6f;

    [Header("Visuals")]
    public Sprite icon;
    public RuntimeAnimatorController animator;

    public MeleeWeapon(MeleeData data)
    {
        weaponName = data.weaponName;
        icon = data.icon;
        damage = data.damage;
        range = data.range;
        cooldown = data.cooldown;
        animator = data.animator;
    }
}