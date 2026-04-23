using UnityEngine;

[System.Serializable]
public class Gun
{
    [Header("Basic Info")]
    public string gunName;

    [Header("Shooting")]
    public float fireRate = 0.2f;
    public int damage = 5;

    [Header("Projectile")]
    public float bulletSpeed = 20f;
    public float bulletLifetime = 1f;

    [Header("Ammo")]
    public int maxAmmo = 10;
    public int currentAmmo;
    public float reloadTime = 1.5f;

    [Header("Visuals")]
    public Sprite baseSprite;
    public Sprite diagonalSprite;

    [Header("Bullet")]
    public GameObject bulletPrefab;

    private float nextFireTime = 0f;

    [Header("Gun Type")]
    public GunType gunType;
    public enum GunType
    {
        OneHanded,
        TwoHanded
    }

    public void Initialize()
    {
        currentAmmo = maxAmmo;
    }

    public bool CanShoot()
    {
        return Time.time >= nextFireTime && currentAmmo > 0;
    }

    public Gun(GunData data)
    {
        gunName = data.gunName;
        fireRate = data.fireRate;
        damage = data.damage;
        bulletSpeed = data.bulletSpeed;
        bulletLifetime = data.bulletLifetime;
        maxAmmo = data.maxAmmo;
        reloadTime = data.reloadTime;
        baseSprite = data.baseSprite;
        diagonalSprite = data.diagonalSprite;
        bulletPrefab = data.bulletPrefab;
        gunType = data.gunType;
    }

    public void Shoot(Transform firePoint, Vector2 direction)
    {
        if (!CanShoot()) return;

        GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(direction);
            b.damage = damage;
            b.speed = bulletSpeed;        
            b.lifetime = bulletLifetime;  
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
    }

    public bool NeedsReload()
    {
        return currentAmmo <= 0;
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }
}