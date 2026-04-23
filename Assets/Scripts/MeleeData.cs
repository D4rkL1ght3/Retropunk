using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Melee")]
public class MeleeData : ScriptableObject
{
    public string weaponName;

    [Header("Stats")]
    public int damage = 10;
    public float range = 0.5f;
    public float cooldown = 0.6f;

    [Header("Animations")]
    public RuntimeAnimatorController animator;
}