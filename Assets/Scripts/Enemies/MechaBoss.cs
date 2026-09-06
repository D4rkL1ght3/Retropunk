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

    [Header("Flight")]
    public Transform flightRayPoint;
    public float flightSpeed = 3f;
    public float flightRayDistance = 10f;
    public float flightOffset = 1f;
    public float flightStoppingDistance = 0.1f;
    public float minimumFlightHeight = 2f;

    private bool isFlying = false;
    private bool reachedTarget = false;
    private Vector2 flightTarget;
    private Vector2 flightMoveDirection;

    [Header("Rocket Exhaust")]
    public Collider2D rocketDamageArea;
    public int rocketDamage = 1;
    public float rocketDamageCooldown = 0.2f;

    public AudioSource flightAudioSource;
    public AudioClip flightSound;

    private float lastRocketDamageTime;

    protected override void Update()
    {
        base.Update();

        if (currentState == EnemyState.Chase && !fightStarted)
        {
            lastMissileTime = Time.time - (missileCooldown / 2f);
            fightStarted = true;
            aggroed = true;
        }

        if (currentState == EnemyState.Chase)
        {
            if (!isFlying)
            {
                CheckForFlight();
                TryMissileBarrage();
            }
            else
            {
                FlyToTarget();
            }
        }
    }

    void TryMissileBarrage()
    {
        if (isAttacking || isFiringMissiles)
            return;

        if (Time.time >= lastMissileTime + missileCooldown && distance > attackRange)
        {
            StartCoroutine(FireMissileBarrage());
            animator.SetTrigger("FireMissiles");
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

    void CheckForFlight()
    {
        if (isFlying)
            return;

        float heightDifference = player.position.y - transform.position.y;

        if (heightDifference < minimumFlightHeight)
            return;

        Vector2 direction = (player.position - flightRayPoint.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(
            flightRayPoint.position,
            direction,
            flightRayDistance,
            Player
        );

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            flightTarget = hit.point + direction * flightOffset;
            isFlying = true;

            animator.SetBool("isFlying", true);

            StartFlightSound();
        }
    }

    void FlyToTarget()
    {
        DealRocketDamage();

        Vector2 direction = flightTarget - (Vector2)transform.position;

        if (direction.magnitude > flightStoppingDistance && !reachedTarget)
        {
            flightMoveDirection = direction.normalized;
        }
        else
        {
            flightMoveDirection = Vector2.zero;
            reachedTarget = true;
            animator.SetTrigger("Land");
        }
    }

    void StartFlightSound()
    {
        if (flightAudioSource == null || flightSound == null)
            return;

        flightAudioSource.clip = flightSound;
        flightAudioSource.loop = true;
        flightAudioSource.Play();
    }

    void StopFlightSound()
    {
        if (flightAudioSource != null && flightAudioSource.isPlaying)
        {
            flightAudioSource.Stop();
        }
    }

    public void LandFlight()
    {
        isFlying = false;
        reachedTarget = false;

        flightMoveDirection = Vector2.zero;

        animator.SetBool("isFlying", false);

        StopFlightSound();
    }

    void DealRocketDamage()
    {
        if (Time.time < lastRocketDamageTime + rocketDamageCooldown)
            return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            rocketDamageArea.bounds.center,
            rocketDamageArea.bounds.size,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(rocketDamage);
            }
        }

        lastRocketDamageTime = Time.time;
    }

    protected override void FixedUpdate()
    {
        if (isFlying)
        {
            if (flightMoveDirection == Vector2.zero)
            {
                Vector2 velocity = rb.linearVelocity;
                rb.linearVelocity = velocity;
                return;
            }

            rb.linearVelocity = flightMoveDirection * flightSpeed;
        }
        else
        {
            base.FixedUpdate();
        }
    }

    protected override void TryAttack()
    {
        if (isFiringMissiles || isFlying)
            return;

        base.TryAttack();
    }

    protected override void DealDamage()
    {
        base.DealDamage();

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    protected override void Flip(float directionX)
    {
        if (isFlying)
            return;

        base.Flip(directionX);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (rocketDamageArea != null)
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawWireCube(
                rocketDamageArea.bounds.center,
                rocketDamageArea.bounds.size
            );
        }

        if (isFlying && flightRayPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(flightTarget, 0.25f);

            Gizmos.DrawLine(
                transform.position,
                flightTarget
            );
        }
    }
}