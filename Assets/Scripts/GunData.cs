using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Gun")]
public class GunData : ScriptableObject
{
    public string gunName;

    [Header("Shooting")]
    public float fireRate = 0.2f;
    public int damage = 5;

    [Header("Projectile")]
    public float bulletSpeed = 20f;
    public float bulletLifetime = 1f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public float reloadTime = 1.5f;

    [Header("Visuals")]
    public Sprite baseSprite;
    public Sprite diagonalSprite;

    [Header("Bullet")]
    public GameObject bulletPrefab;

    public Gun.GunType gunType;
}