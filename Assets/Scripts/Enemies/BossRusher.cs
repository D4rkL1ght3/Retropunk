using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR.Haptics;

public class BossRusher : EnemyRusher
{
    private BossController boss;
    private bool wasRushingLastFrame;

    [Header("Boss Settings")]
    public float patrolSpeed = 2.5f;
    public bool fightStarted = false;
    public float knockbackForce = 10f;
    public float stunDuration = 1f;

    protected override void Start()
    {
        base.Start();
        boss = GetComponentInParent<BossController>();
    }

    protected override void Update()
    {
        base.Update();

        DetectRushEnd();

        if (currentState == EnemyState.Chase && !fightStarted)
        {
            boss.StartBossFight();
            aggroed = true;
            fightStarted = true;
        }
    }

    protected override void FixedUpdate()
    {
        if (currentState == EnemyState.Patrol)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.x = moveDirection * patrolSpeed;
            rb.linearVelocity = velocity;
            return;
        }

        base.FixedUpdate();
    }

    protected override void RushHit()
    {
        base.RushHit();

        PlayerController controller = player.GetComponent<PlayerController>();

        if (controller != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            controller.ApplyKnockback(dir, knockbackForce);
            controller.ApplyStun(stunDuration);
        }
    }

    void DetectRushEnd()
    {
        if (!isRushing && wasRushingLastFrame)
        {
            boss.OnRushFinished();
        }

        wasRushingLastFrame = isRushing;
    }
}