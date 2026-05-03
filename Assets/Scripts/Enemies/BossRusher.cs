using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR.Haptics;

public class BossRusher : EnemyRusher
{
    private BossController boss;
    private bool wasRushingLastFrame;
    public float patrolSpeed = 2.5f;

    protected override void Start()
    {
        base.Start();
        boss = GetComponentInParent<BossController>();
    }

    protected override void Update()
    {
        base.Update();

        DetectRushEnd();

        if (currentState == EnemyState.Chase)
        {
            aggroed = true;
            boss.StartBossFight();
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

    void DetectRushEnd()
    {
        if (!isRushing && wasRushingLastFrame)
        {
            boss.OnRushFinished();
        }

        wasRushingLastFrame = isRushing;
    }
}