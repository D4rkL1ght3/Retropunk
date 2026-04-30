using UnityEngine;

public class BossRusher : EnemyRusher
{
    private BossController boss;
    private bool wasRushingLastFrame;

    protected override void Start()
    {
        base.Start();
        boss = GetComponentInParent<BossController>();
    }

    protected override void Update()
    {
        base.Update();

        DetectRushEnd();
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