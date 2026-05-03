using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;

public class BossMelee : EnemyMelee
{
    private BossController boss;

    protected override void Start()
    {
        base.Start();
        boss = GetComponentInParent<BossController>();
        aggroed = true;
        aggroTime = Mathf.Infinity;
    }

    protected override void Update()
    {
        if (boss == null) return;

        if (boss.IsReturningToCart)
        {
            HandleReturnToCart();
            return;
        }

        base.Update();
    }

    void HandleReturnToCart()
    {
        float dir = Mathf.Sign(boss.CartPosition.x - transform.position.x);
        distance = Vector2.Distance(transform.position, player.position);

        // Override movement toward cart
        moveDirection = dir;
        animator.SetBool("isMoving", moveDirection != 0f);

        if (isAttacking)
        {
            Flip(player.position.x - transform.position.x);
        }
        else
        {
            Flip(dir);
        }

        // STILL allow attacking
        TryAttack();

        // Reached cart
        if (Vector2.Distance(transform.position, boss.CartPosition) < 0.2f && !isAttacking)
        {
            boss.TriggerRemount();
        }
    }

    void OnDestroy()
    {
        if (boss != null)
        {
            boss.OnBossDeath();
            Destroy(boss.gameObject);
        }
    }
}