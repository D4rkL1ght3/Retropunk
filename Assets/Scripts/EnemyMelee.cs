using UnityEditor.SceneManagement;
using UnityEngine;

public class EnemyMelee : MonoBehaviour, IEntity
{
    public Transform player;

    public float patrolDistance = 3f;
    private float patrolLeftX;
    private float patrolRightX;

    private int patrolDirection = 1;

    public float moveSpeed = 3f;
    public float detectionRange = 6f;
    public float attackRange = 1.5f;
    public LayerMask Player;
    public LayerMask Ground;

    private bool aggroed = false;
    private Transform currentTarget;
    private Rigidbody2D rb;
    private Health health;

    enum EnemyState
    {
        Patrol,
        Chase
    }

    EnemyState currentState = EnemyState.Patrol;

    public float patrolWaitTime = 1.5f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    public int damage = 4;
    public float attackCooldown = 1.2f;

    private float distance;
    private float lastAttackTime;
    bool isAttacking = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        patrolLeftX = transform.position.x - patrolDistance;
        patrolRightX = transform.position.x + patrolDistance;

        if (transform.position.x > patrolRightX)
            patrolDirection = -1;
        else
            patrolDirection = 1;
    }

    void Update()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
        
        if ((distance < detectionRange && canSeePlayer) || aggroed)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            aggroed = false;
            currentState = EnemyState.Patrol;
        }

        if (currentState == EnemyState.Chase)
        {
            Flip(player.position.x - transform.position.x);
        }
        else if (currentState == EnemyState.Patrol)
        {
            Flip(patrolDirection);
        }

        if (distance >= attackRange && currentState == EnemyState.Chase)
        {
            animator.SetBool("isMoving", true);
        }
        else if (currentState == EnemyState.Chase)
        {
            animator.SetBool("isMoving", false);
            TryAttack();
        }
    }

    void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;

            // Keep facing player while attacking
            if (player != null)
            {
                Flip(player.position.x - transform.position.x);
            }

            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
            break;

            case EnemyState.Chase:
                if (distance >= attackRange)
                    ChasePlayer();
            break;
        }
    }

    void Patrol()
    {
        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);

            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                isWaiting = false;
                patrolDirection *= -1;
            }

            return;
        }

        rb.linearVelocity = new Vector2(patrolDirection * moveSpeed, rb.linearVelocity.y);
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

    void ChasePlayer()
    {
        Vector2 direction = player.position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void Flip(float directionX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(directionX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
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
        float distance = Vector2.Distance(transform.position, player.position);

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
        currentState = EnemyState.Chase;
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