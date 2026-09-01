using UnityEngine;
using System.Collections;

public class MechaBoss : EnemyMelee
{
    [Header("Seeking Missile Barrage")]
    public GameObject missilePrefab;
    public Transform missileLaunchPoint;

    public int missileCount = 6;
    public float missileCooldown = 4f;
    public float missileBurstDelay = 0.2f;
    public float missileStartingAngle = 0f;

    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip fireSound;

    private float lastMissileTime;
    private bool isFiringMissiles;
    private bool fightStarted = false;

    protected override void Update()
    {
        base.Update();

        if (currentState == EnemyState.Chase && !fightStarted)
        {
            lastMissileTime = Time.time + missileCooldown;
            fightStarted = true;
            aggroed = true;
        }

        if (currentState == EnemyState.Chase)
        {
            TryMissileBarrage();
        }
    }

    void TryMissileBarrage()
    {
        if (isFiringMissiles)
            return;

        if (Time.time >= lastMissileTime + missileCooldown)
        {
            StartCoroutine(FireMissileBarrage());

            lastMissileTime = Time.time;
        }
    }

    IEnumerator FireMissileBarrage()
    {
        isFiringMissiles = true;

        for (int i = 0; i < missileCount; i++)
        {
            FireMissile();

            if (audioSource != null && fireSound != null)
            {
                audioSource.PlayOneShot(fireSound);
            }

            if (i < missileCount - 1)
            {
                yield return new WaitForSeconds(missileBurstDelay);
            }
        }

        isFiringMissiles = false;
    }

    void FireMissile()
    {
        float facingDirection = Mathf.Sign(transform.localScale.x);

        float launchAngle = missileStartingAngle;

        if (facingDirection < 0)
        {
            launchAngle = 180f - missileStartingAngle;
        }

        Quaternion launchRotation = Quaternion.Euler(
            0f,
            0f,
            launchAngle
        );

        GameObject missile = Instantiate(
            missilePrefab,
            missileLaunchPoint.position,
            launchRotation
        );

        SeekingMissile seekingMissile = missile.GetComponent<SeekingMissile>();

        if (seekingMissile != null)
        {
            seekingMissile.player = player;
        }
    }

    protected override void DealDamage()
    {
        base.DealDamage();

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}