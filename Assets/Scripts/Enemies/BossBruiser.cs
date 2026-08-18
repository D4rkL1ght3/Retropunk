using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BossBruiser : EnemyMelee
{
    [Header("Boss Charge")]
    public float sprintSpeed = 6f;
    private float walkSpeed;

    public float chargeRange = 5f;
    public float heavyCooldown = 6f;
    public int heavyDamage = 12;
    public float heavyAttackRange = 0.8f;

    public float knockbackForce = 12f;
    public float stunDuration = 1f;

    [Header("Football Throw")]
    public GameObject lobbedProjectilePrefab;
    public Transform throwPoint;

    public float throwRange = 10f;
    public float throwForce = 7f;
    public float throwCooldown = 3f;

    private bool heavyReady;
    private bool throwReady;
    private bool isCharging = false;
    private bool isTaunting = false;
    private bool isThrowing = false;

    private float nextHeavyTime;
    private float nextThrowTime;
    private bool fightStarted = false;

    [Header("References")]
    [SerializeField] private CinemachineConfiner2D cameraConfiner;
    public BoxCollider2D bossCameraBounds;
    private BoxCollider2D levelCameraBounds;

    [SerializeField] private Goal goal;
    public GameObject doorClosed;
    public GameObject doorOpen;

    protected override void Start()
    {
        base.Start();

        GetComponent<Health>().OnDeath += OnBossDeath;

        walkSpeed = moveSpeed;

        if (cameraConfiner != null)
        {
            levelCameraBounds = cameraConfiner.BoundingShape2D as BoxCollider2D;
        }
    }

    protected override void Update()
    {
        if (isTaunting)
        {
            moveDirection = 0f;
            return;
        }

        base.Update();

        if (currentState == EnemyState.Chase && !fightStarted)
        {
            StartBossFight();
            aggroed = true;
            fightStarted = true;
        }

        heavyReady = Time.time >= nextHeavyTime;
        throwReady = Time.time >= nextThrowTime;

        if (heavyReady && !isCharging && distance <= chargeRange)
        {
            moveSpeed = sprintSpeed;
            animator.SetBool("isSprinting", true);
            isCharging = true;
        }

        if (throwReady && !isThrowing && !isCharging)
        {
            TryThrow();
        }
    }

    protected override void ChasePlayer()
    {
        if (isAttacking || isThrowing)
        {
            moveDirection = 0f;
            return;
        }

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        if (heavyReady && distance > heavyAttackRange)
        {
            moveDirection = dir;
        }
        else if (!heavyReady && distance > attackRange)
        {
            moveDirection = dir;
        }
        else
        {
            moveDirection = 0f;
        }
    }

    protected override void TryAttack()
    {
        if (isThrowing) return;

        if (heavyReady)
        {
            if (Time.time >= lastAttackTime + attackCooldown && distance <= heavyAttackRange)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        else
        {
            base.TryAttack();
        }
    }

    protected override void Attack()
    {
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        if (heavyReady)
        {
            animator.SetTrigger("HeavyAttack");
            nextHeavyTime = Time.time + heavyCooldown;

            moveSpeed = walkSpeed;
            animator.SetBool("isSprinting", false);
            isCharging = false;
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    public void DealHeavyDamage()
    {
        distance = Vector2.Distance(transform.position, player.position);

        if (distance <= heavyAttackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(heavyDamage);
            }

            PlayerController controller = player.GetComponent<PlayerController>();

            if (controller != null)
            {
                Vector2 dir = (player.position - transform.position).normalized;

                controller.ApplyKnockback(dir, knockbackForce);
                controller.ApplyStun(stunDuration);
            }

            isTaunting = true;
            animator.SetBool("isTaunting", true);
        }
    }

    public void EndTaunt()
    {
        animator.SetBool("isTaunting", false);
        isTaunting = false;
    }

    void TryThrow()
    {
        if (isAttacking) return;

        if (throwReady && distance > chargeRange && distance <= throwRange)
        {
            isThrowing = true;
            animator.SetTrigger("Throw");
            nextThrowTime = Time.time + throwCooldown;
        }
    }

    public void ThrowLobbedProjectile()
    {
        float horizontalDistance = player.position.x - throwPoint.position.x;

        Vector2 throwVelocity = new Vector2(
            horizontalDistance,
            throwForce
        );

        GameObject projectile = Instantiate(
            lobbedProjectilePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        LobbedProjectile lobbedProjectile =
            projectile.GetComponent<LobbedProjectile>();

        if (lobbedProjectile != null)
        {
            lobbedProjectile.Initialize(throwVelocity);
        }
    }

    public void EndThrow()
    {
        isThrowing = false;
    }

    public void StartBossFight()
    {
        AudioManager.Instance.PlayBossMusic();

        if (cameraConfiner != null && bossCameraBounds)
        {
            cameraConfiner.BoundingShape2D = bossCameraBounds;
        }
    }

    public void OnBossDeath()
    {
        if (goal != null)
            goal.UnlockGoal();

        if (cameraConfiner != null)
            cameraConfiner.BoundingShape2D = levelCameraBounds;

        if (doorOpen && doorClosed != null)
        {
            doorClosed.SetActive(false);
            doorOpen.SetActive(true);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Heavy attack range
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, heavyAttackRange);
        // Charge range
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        // Throw range
        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, throwRange);
    }
}