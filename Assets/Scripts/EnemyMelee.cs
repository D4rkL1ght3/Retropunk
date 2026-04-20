using UnityEngine;

public class EnemyMelee : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    private float moveDirection;

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

    private float lastAttackTime;
    private bool isAttacking = false;

    [Header("Raycast")]
    public LayerMask Player;
    public LayerMask Ground;
    public LayerMask Platform;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    [SerializeField] float maxDropHeight = 3f;
    [SerializeField] float maxReachableHeight = 1.5f;

    [Header("Aggro")]
    public float aggroTime = 3f;
    private bool aggroed = false;
    private float aggroTimer;
    private float distance;

    enum EnemyState
    {
        Patrol,
        Chase
    }

    EnemyState currentState = EnemyState.Patrol;

    void Start()
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
    }

    void Update()
    {
        distance = Vector2.Distance(transform.position, player.position);
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
        
        if (((distance <= detectionRange && canSeePlayer) || aggroed) && canReachPlayer)
        {
            aggroTimer = aggroTime; // Reset aggro timer
            currentState = EnemyState.Chase;
        }
        else
        {
            aggroTimer -= Time.deltaTime;
            if (aggroTimer <= 0)
            {
                currentState = EnemyState.Patrol;
                aggroed = false; // Reset aggro state when timer runs out
            }
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
        Flip(moveDirection);
    }

    void FixedUpdate()
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

    void ChasePlayer()
    {
        if (isAttacking)
        {
            moveDirection = 0f;
            return;
        }

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        if (distance > attackRange && IsDropSafe(dir))
        {
            moveDirection = dir;
        }
        else
        {
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

    void Flip(float directionX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

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

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void DealDamage()
    {
        distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
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
}