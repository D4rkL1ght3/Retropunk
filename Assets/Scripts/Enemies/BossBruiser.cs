using System.Collections;
using UnityEngine;

public class BossBruiser : EnemyMelee
{
    [Header("Heavy Charge")]
    private float walkSpeed;
    public float sprintSpeed = 6f;

    public float heavyCooldown = 6f;
    public int heavyDamage = 12;
    public float knockbackForce = 12f;
    public float stunDuration = 1f;

    public float heavyRecovery = 1f;
    public float heavyWindup = 0.35f;

    private bool heavyReady;
    private bool isCharging = false;
    private float nextHeavyTime;

    protected override void Start()
    {
        base.Start();

        walkSpeed = moveSpeed;
    }

    protected override void Update()
    {
        heavyReady = Time.time >= nextHeavyTime;

        if (heavyReady && !isCharging)
        {
            moveSpeed = sprintSpeed;
            animator.SetBool("IsSprinting", true);
            isCharging = true;
        }

        base.Update();
    }

    protected override void Attack()
    {
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        if (heavyReady)
        {
            animator.SetBool("IsSprinting", false);
            animator.SetTrigger("HeavyAttack");

            nextHeavyTime = Time.time + heavyCooldown;
            moveSpeed = walkSpeed;
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

        if (distance <= attackRange)
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
        }
    }
}