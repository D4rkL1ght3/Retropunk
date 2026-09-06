using UnityEngine;

public class EnemyMelee : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    protected float moveDirection;

    [Header("Patrol")]
    public float patrolDistance = 3f;
    public float patrolWaitTime = 1.5f;

    private float patrolLeftX;
    private float patrolRightX;
    private int patrolDirection = 1;
    private float waitTimer;
    private bool isWaiting = false;
    public float detectionRange = 8f;

    [Header("Attacking")]
    public float attackRange = 0.8f;
    public int damage = 4;
    public float attackCooldown = 1.2f;

    public bool useAttackPoint = false;
    public Transform attackPoint;

    [SerializeField] private float flipDeadzone = 0.2f;
    private float lastRepositionDirection;

    protected private float lastAttackTime;
    protected bool isAttacking = false;

    [Header("Raycast")]
    public LayerMask Player;
    public LayerMask Ground;
    public LayerMask Platform;

    protected Rigidbody2D rb;
    protected Animator animator;
    private Health health;

    [SerializeField] float maxDropHeight = 3f;
    [SerializeField] float maxReachableHeight = 1.5f;

    [Header("Aggro")]
    public float aggroTime = 3f;
    public bool aggroed = false;
    private float aggroTimer;
    protected float distance;

    protected enum EnemyState
    {
        Patrol,
        Chase
    }

    protected EnemyState currentState = EnemyState.Patrol;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        patrolLeftX = transform.position.x - patrolDistance;
        patrolRightX = transform.position.x + patrolDistance;
        lastRepositionDirection = Mathf.Sign(transform.localScale.x);
    }

    protected virtual void Update()
    {
        distance = Vector2.Distance(GetAttackOrigin(), player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        // Raycast toward player
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            detectionRange,
            Ground | Player
        );

        bool canSeePlayer = false;
        bool canReachPlayer = canChase();

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }
        
        if (((distance <= detectionRange && canSeePlayer) && canReachPlayer) || aggroed)
        {
            aggroTimer = aggroTime; // Reset aggro timer
            currentState = EnemyState.Chase;
        }
        else
        {
            if (!aggroed)
                aggroTimer -= Time.deltaTime; // Countdown aggro timer

            if (aggroTimer <= 0)
                currentState = EnemyState.Patrol;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                TryAttack();
                break;
        }

        animator.SetBool("isMoving", moveDirection != 0f);
        if (currentState == EnemyState.Chase)
        {
            Flip(player.position.x - transform.position.x);
        }
        else
        {
            Flip(patrolDirection);
        }
    }

    protected virtual void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveDirection * moveSpeed;
        rb.linearVelocity = velocity;
    }

    void Patrol()
    {
        if (isWaiting)
        {
            moveDirection = 0f;

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;
                patrolDirection *= -1;
            }

            return;
        }

        moveDirection = patrolDirection;

        if (patrolDirection == 1 && transform.position.x >= patrolRightX)
        {
            isWaiting = true;
            waitTimer = patrolWaitTime;
        }

        if (patrolDirection == -1 && transform.position.x <= patrolLeftX)
        {
            isWaiting = true;
            waitTimer = patrolWaitTime;
        }
    }

    protected virtual void ChasePlayer()
    {
        if (isAttacking)
        {
            moveDirection = 0f;
            return;
        }

        // Normal melee enemy behavior
        if (!useAttackPoint || attackPoint == null)
        {
            float dir = Mathf.Sign(player.position.x - transform.position.x);

            if (distance > attackRange && IsDropSafe(dir))
            {
                moveDirection = dir;
            }
            else
            {
                moveDirection = 0f;
            }

            return;
        }

        // Attack-point enemy behavior
        if (distance > attackRange)
        {
            float dir = GetPlayerDirection();

            if (dir != 0f && IsDropSafe(dir))
                moveDirection = dir;
            else
                moveDirection = 0f;
        }
        else if (IsPlayerBehindAttackPoint())
        {
            float retreatDirection = -GetAttackPointFacingDirection();

            if (IsDropSafe(retreatDirection))
                moveDirection = retreatDirection;
            else
                moveDirection = 0f;
        }
    }

    bool canChase()
    {
        return CanReachPlayerHeight() && IsDropSafe(Mathf.Sign(player.position.x - transform.position.x));
    }

    bool CanReachPlayerHeight()
    {
        float verticalDifference = player.position.y - transform.position.y;
        return verticalDifference <= maxReachableHeight;
    }

    bool IsDropSafe(float direction)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(direction * 0.3f, 0);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            Mathf.Infinity,
            Ground | Platform
        );

        if (hit.collider == null)
            return false; // no ground at all = definitely unsafe

        float dropHeight = origin.y - hit.point.y;

        return dropHeight <= maxDropHeight;
    }

    protected virtual void Flip(float directionX)
    {
        if (Mathf.Abs(directionX) <= flipDeadzone)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    protected virtual void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown && distance <= attackRange)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack()
    {
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    protected virtual void DealDamage()
    {
        distance = Vector2.Distance(GetAttackOrigin(), player.position);

        if (distance <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    protected float GetPlayerDirection()
    {
        float horizontalDifference = player.position.x - transform.position.x;

        if (Mathf.Abs(horizontalDifference) <= flipDeadzone)
            return lastRepositionDirection;

        lastRepositionDirection = Mathf.Sign(horizontalDifference);

        return lastRepositionDirection;
    }

    protected float GetAttackPointFacingDirection()
    {
        float direction = attackPoint.position.x - transform.position.x;

        if (Mathf.Abs(direction) <= flipDeadzone)
            return Mathf.Sign(transform.localScale.x);

        return Mathf.Sign(direction);
    }

    protected bool IsPlayerBehindAttackPoint()
    {
        float facingDirection = GetAttackPointFacingDirection();
        float playerFromAttackPoint = player.position.x - attackPoint.position.x;

        if (Mathf.Abs(playerFromAttackPoint) <= flipDeadzone)
            return false;

        return Mathf.Sign(playerFromAttackPoint) != facingDirection;
    }

    protected Vector2 GetAttackOrigin()
    {
        if (useAttackPoint && attackPoint != null)
            return attackPoint.position;

        return transform.position;
    }

    public void OnDamaged()
    {
        aggroed = true;
    }
    public void Disable()
    {
        enabled = false;
        health.enabled = false;
    }
    public void OnDeath()
    {
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackOrigin(), attackRange);

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Patrol range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolDistance);

        if (useAttackPoint)
        {
            // Attack range deadzone
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, flipDeadzone);
        }
    }
}