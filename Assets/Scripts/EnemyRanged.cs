using UnityEngine;

public class EnemyRanged : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    private float moveDirection;

    [Header("Patrol")]
    public float patrolDistance = 3f;
    public float patrolWaitTime = 1.5f;

    private float patrolLeftX;
    private float patrolRightX;
    private int patrolDirection = 1;
    private float waitTimer;
    private bool isWaiting = false;

    [Header("Ranges")]
    public float detectionRange = 8f;
    public float optimalRange = 5f;
    public float retreatRange = 3f;

    [Header("Shooting")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Raycast")]
    public LayerMask Player;
    public LayerMask Ground;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    private bool aggroed = false;
    private float distance;

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

        patrolLeftX = transform.position.x - patrolDistance;
        patrolRightX = transform.position.x + patrolDistance;
    }

    void Update()
    {
        distance = Vector2.Distance(transform.position, player.position);

        bool canSeePlayer = HasClearShot();

        if ((distance < detectionRange && canSeePlayer) || aggroed)
        {
            aggroed = true;
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;

            case EnemyState.Chase:
                HandleMovement();
                TryShoot();
                break;
        }

        if (currentState == EnemyState.Chase)
        {
            Flip(player.position.x - transform.position.x);
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

    // ================= PATROL =================

    void HandlePatrol()
    {
        if (isWaiting)
        {
            moveDirection = 0f;
            animator.SetBool("isMoving", false);

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;
                patrolDirection *= -1;
            }

            return;
        }

        moveDirection = patrolDirection;
        animator.SetBool("isMoving", true);

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

    // ================= COMBAT =================

    void HandleMovement()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        if (distance > optimalRange)
        {
            moveDirection = dir;
            animator.SetBool("isMoving", true);
        }
        else if (distance < retreatRange)
        {
            moveDirection = -dir;
            animator.SetBool("isMoving", true);
        }
        else
        {
            moveDirection = 0f;
            animator.SetBool("isMoving", false);
        }
    }

    void TryShoot()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (HasClearShot())
            {
                Shoot();
                lastAttackTime = Time.time;
            }
        }
    }

    void Shoot()
    {
        animator.SetTrigger("Shoot");

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        if (b != null)
        {
            b.Initialize(direction);
        }
    }

    // ================= HELPERS =================

    bool HasClearShot()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;
        float dist = Vector2.Distance(firePoint.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            dist,
            Ground | Player
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
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
}