using UnityEngine;

public class EnemyFlying : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float hoverHeight = 1f;
    public float stoppingDistance = 0.1f;

    private Vector2 moveDirection;

    [Header("Patrol")]
    public float patrolRadius = 2.5f;
    public float patrolWaitTime = 1.5f;

    private Vector2 patrolCenter;
    private Vector2 patrolTarget;
    private float waitTimer;
    private bool isWaiting = false;

    [Header("Detection")]
    public float detectionRange = 8f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.3f;
    public int damage = 2;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Aggro")]
    public float aggroTime = 1f;
    private bool aggroed = false;
    private float aggroTimer;

    private float distance;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    enum EnemyState
    {
        Patrol,
        Chase
    }

    EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        patrolCenter = transform.position;
        PickNewPatrolPoint();
    }

    void Update()
    {
        distance = Vector2.Distance(attackPoint.position, player.position);

        // Detection
        if (distance <= detectionRange || aggroed)
        {
            aggroTimer = aggroTime;
            currentState = EnemyState.Chase;
        }
        else
        {
            aggroTimer -= Time.deltaTime;
            if (aggroTimer <= 0)
            {
                currentState = EnemyState.Patrol;
                aggroed = false;
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                TryAttack();
                break;
        }

        animator.SetBool("isMoving", moveDirection.magnitude > 0.1f);
        Flip();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // ================= PATROL =================

    void HandlePatrol()
    {
        if (isWaiting)
        {
            moveDirection = Vector2.zero;

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;
                PickNewPatrolPoint();
            }

            return;
        }

        Vector2 dir = (patrolTarget - (Vector2)transform.position);

        if (dir.magnitude <= 0.2f)
        {
            isWaiting = true;
            waitTimer = patrolWaitTime;
            moveDirection = Vector2.zero;
        }
        else
        {
            moveDirection = dir.normalized;
        }
    }

    void PickNewPatrolPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = patrolCenter + randomOffset;
    }

    // ================= CHASE =================

    void ChasePlayer()
    {
        if (isAttacking)
        {
            moveDirection = Vector2.zero;
            return;
        }

        Vector2 targetPos = (Vector2)player.position + Vector2.up * hoverHeight;
        Vector2 dir = targetPos - (Vector2)transform.position;

        if (dir.magnitude > stoppingDistance)
        {
            moveDirection = dir.normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    // ================= ATTACK =================

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown && distance <= attackRange)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Attack");
    }

    public void DealDamage()
    {
        distance = Vector2.Distance(attackPoint.position, player.position);

        if (distance <= attackRange)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    // ================= HELPERS =================

    void Flip()
    {
        float dirX = player.position.x - transform.position.x;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(dirX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void OnDamaged()
    {
        aggroed = true;
    }

    public void Disable()
    {
        rb.gravityScale = 4f;
        enabled = false;
        health.enabled = false;
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
    }
}