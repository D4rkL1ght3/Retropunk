using UnityEngine;

public class EnemyHitAndRun : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 4f;
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
    public float rushDuration = 3f;
    public float rushRange = 3f;
    public float attackRange = 0.8f;
    public int damage = 8;

    private bool isRushing = false;
    private float rushTimer;

    private float rushDirection;
    private bool hasHitThisRun = false;

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
    public float aggroTime = 5f;
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
                if (isRushing)
                {
                    HandleRush();
                    TryAttack();
                }
                else
                {
                    ChasePlayer();

                    if (distance <= rushRange)
                    {
                        StartRush();
                    }
                }
                break;
        }

        animator.SetBool("isMoving", moveDirection != 0f);
        if (currentState == EnemyState.Chase)
        {
            Flip(moveDirection);
        }
        else
        {
            Flip(patrolDirection);
        }
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

    void StartRush()
    {
        isRushing = true;
        rushTimer = rushDuration;
        hasHitThisRun = false;
        rushDirection = Mathf.Sign(player.position.x - transform.position.x);
        animator.SetBool("isRushing", true);
    }

    void HandleRush()
    {
        moveDirection = rushDirection;

        rushTimer -= Time.deltaTime;

        if (rushTimer <= 0 || HitWall(rushDirection))
        {
            isRushing = false;
            animator.SetBool("isRushing", false);
        }
    }

    void ChasePlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        if (distance > rushRange && IsDropSafe(dir))
        {
            moveDirection = dir;
        }
        else
        {
            moveDirection = 0f;
        }
    }

    void TryAttack()
    {
        if (!hasHitThisRun && distance <= attackRange)
        {
            RushHit();
            hasHitThisRun = true;
        }
    }

    void RushHit()
    {
        if (!isRushing || hasHitThisRun) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
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

    bool HitWall(float direction)
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(direction * 0.3f, 0);
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * direction,
            0.2f,
            Ground
        );
        return hit.collider != null;
    }

    void Flip(float directionX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
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

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // Rush range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rushRange);
        // Attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}