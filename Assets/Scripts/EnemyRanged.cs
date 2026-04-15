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
    private bool isShooting = false;

    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Raycast")]
    public LayerMask Player;
    public LayerMask Ground;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

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

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
            }
        }

        if ((distance <= detectionRange && canSeePlayer) || aggroed)
        {
            aggroTimer = aggroTime; // Reset aggro timer
            currentState = EnemyState.Chase;
        }
        else
        {
            aggroTimer -= Time.deltaTime;
            if (aggroTimer <= 0)
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

    // ================= COMBAT =================

    void HandleMovement()
    {
        if (isShooting)
        {
            moveDirection = 0f;
            return;
        }

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        bool canShoot = CanShootPlayer();

        if (distance > optimalRange)
        {
            moveDirection = dir;
        }
        else if (distance <= retreatRange)
        {
            moveDirection = -dir;
        }
        else if (distance <= optimalRange && !canShoot)
        {
            moveDirection = dir;
        }
        else
        {
            moveDirection = 0f;
        }
    }

    void TryShoot()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Vector2 rawDir = (player.position - firePoint.position);
            Vector2 snappedDir = GetSnappedDirection(rawDir);

            if (snappedDir != Vector2.zero && HasClearShot(snappedDir))
            {
                Shoot(snappedDir);
                isShooting = true;
                lastAttackTime = Time.time;
            }
        }
    }

    void Shoot(Vector2 direction)
    {
        Flip(direction.x);
        PlayShootAnimation(direction);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        if (b != null)
        {
            b.Initialize(direction);
        }
    }

    void PlayShootAnimation(Vector2 dir)
    {
        if (dir.y > 0.5f)
        {
            animator.SetTrigger("ShootUp");
        }
        else if (dir.y < -0.5f)
        {
            animator.SetTrigger("ShootDown");
        }
        else
        {
            animator.SetTrigger("Shoot");
        }
    }

    public void EndShoot()
    {
        isShooting = false;
    }

    // ================= HELPERS =================

    Vector2 GetSnappedDirection(Vector2 rawDir)
    {
        float angle = Mathf.Atan2(rawDir.y, rawDir.x) * Mathf.Rad2Deg;

        // Snap to nearest 45 degrees
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;

        // FORWARD
        if (snappedAngle == 0 || snappedAngle == 180)
            return new Vector2(Mathf.Sign(rawDir.x), 0);

        // UP DIAGONAL
        if (snappedAngle == 45 || snappedAngle == 135)
            return new Vector2(Mathf.Sign(rawDir.x), 1).normalized;

        // DOWN DIAGONAL
        if (snappedAngle == -45 || snappedAngle == -135)
            return new Vector2(Mathf.Sign(rawDir.x), -1).normalized;

        return Vector2.zero;
    }

    Vector2 GetCurrentSnappedDirection()
    {
        Vector2 rawDir = player.position - firePoint.position;
        return GetSnappedDirection(rawDir);
    }

    bool CanShootPlayer()
    {
        Vector2 snappedDir = GetCurrentSnappedDirection();
        return snappedDir != Vector2.zero && HasClearShot(snappedDir);
    }

    bool HasClearShot(Vector2 direction)
    {
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