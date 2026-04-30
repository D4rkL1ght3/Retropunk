using UnityEngine;
using System.Collections;

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

    [Header("Shooting Mode")]
    public bool horizontalOnly = false;
    public int burstCount = 3;
    public float burstDelay = 0.5f;

    [Header("Raycast")]
    public LayerMask Player;
    public LayerMask Ground;
    public LayerMask Platform;

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    [SerializeField] float maxDropHeight = 2f;

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

        animator.SetBool("isMoving", moveDirection != 0f && IsDropSafe(moveDirection));
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
        if (IsDropSafe(moveDirection))
            velocity.x = moveDirection * moveSpeed;
        else
            velocity.x = 0f;
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

        bool canShoot;

        if (!horizontalOnly)
        {
            canShoot = HasClearShot(GetCurrentSnappedDirection());
        }
        else
        {
            canShoot = HasClearShot(new Vector2(dir, 0));
        }

        if (canShoot || horizontalOnly)
        {
            if (distance < retreatRange)
                moveDirection = -dir;
            else if (distance > optimalRange)
                moveDirection = dir;
            else
                moveDirection = 0f;

            return;
        }
        else
        {
            moveDirection = GetRepositionDirection();
        }
    }

    void TryShoot()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Vector2 shootDir;

            if (horizontalOnly)
            {
                float dirX = Mathf.Sign(player.position.x - firePoint.position.x);
                shootDir = new Vector2(dirX, 0);
            }
            else
            {
                shootDir = GetCurrentSnappedDirection();
            }

            if (HasClearShot(shootDir))
            {
                if (burstCount > 1)
                {
                    StartCoroutine(BurstShoot(shootDir));
                }
                else
                {
                    Shoot(shootDir);
                    isShooting = true;
                }

                lastAttackTime = Time.time;
            }
        }
    }

    void Shoot(Vector2 direction)
    {
        Flip(direction.x);

        if (!horizontalOnly)
        {
            PlayShootAnimation(direction);
        }
        else
        {
            animator.SetTrigger("Shoot");
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        EnemyBullet b = bullet.GetComponent<EnemyBullet>();
        if (b != null)
        {
            b.Initialize(direction);
        }
    }

    IEnumerator BurstShoot(Vector2 direction)
    {
        for (int i = 0; i < burstCount; i++)
        {
            isShooting = true;
            Shoot(direction);

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
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

    bool HasClearShot(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            detectionRange,
            Ground | Player
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    bool TryGetShotHit(Vector2 direction, out Vector2 hitPoint)
    {
        float maxDistance = detectionRange;
        Vector2 origin = firePoint.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            maxDistance,
            Ground | Player
        );

        if (hit.collider != null)
        {
            hitPoint = hit.point;
            return true;
        }
        else
        {
            hitPoint = origin + direction;
            return true;
        }
    }

    float GetRepositionDirection()
    {
        Vector2 dir = GetCurrentSnappedDirection();

        if (TryGetShotHit(dir, out Vector2 hitPoint))
        {
            // If this shot already hits player, no need to move
            if (HasClearShot(dir))
                return 0f;

            // Move to align hit point with player
            float deltaX = player.position.x - hitPoint.x;
            if (Mathf.Abs(deltaX) < 0.2f)
                return 0f;

            return Mathf.Sign(deltaX);
        }

        return 0f;
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
        // Optimal range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, optimalRange);
        // Retreat range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, retreatRange);
    }
}