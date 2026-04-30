using UnityEngine;

public class BossMelee : EnemyMelee
{
    private BossController boss;

    protected override void Start()
    {
        base.Start();
        boss = GetComponentInParent<BossController>();
    }

    protected override void Update()
    {
        base.Update();

        if (boss == null) return;

        if (boss.IsReturningToCart)
        {
            HandleReturnToCart();
        }
    }

    void HandleReturnToCart()
    {
        float dir = Mathf.Sign(boss.CartPosition.x - transform.position.x);

        // Override movement toward cart
        moveDirection = dir;

        // STILL allow attacking
        TryAttack();

        // Reached cart
        if (Vector2.Distance(transform.position, boss.CartPosition) < 0.5f)
        {
            boss.TriggerRemount();
        }
    }
}