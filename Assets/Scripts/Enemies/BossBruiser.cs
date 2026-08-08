using System.Collections;
using UnityEngine;

public class BossBruiser : EnemyMelee
{
    [Header("Heavy Charge")]
    private float walkSpeed;
    public float sprintSpeed = 6f;

    public float chargeRange = 5f;
    public float heavyCooldown = 6f;
    public int heavyDamage = 12;
    public float heavyAttackRange = 0.8f;
    public float knockbackForce = 12f;
    public float stunDuration = 1f;

    private bool heavyReady;
    private bool isCharging = false;
    private bool isTaunting = false;
    private float nextHeavyTime = -1f;
    private bool fightStarted = false;

    protected override void Start()
    {
        base.Start();

        walkSpeed = moveSpeed;
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

        if (heavyReady && !isCharging && distance <= chargeRange)
        {
            moveSpeed = sprintSpeed;
            animator.SetBool("isSprinting", true);
            isCharging = true;
        }
    }

    protected override void ChasePlayer()
    {
        if (isAttacking)
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

    public void StartBossFight()
    {
        AudioManager.Instance.PlayBossMusic();
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
    }
}