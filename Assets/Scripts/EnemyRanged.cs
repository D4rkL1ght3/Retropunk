using UnityEngine;

public class EnemyRangedAI : MonoBehaviour, IEntity
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    private float moveDirection;

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
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator animator;

    private bool aggroed = false;
    private float distance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        distance = Vector2.Distance(transform.position, player.position);

        if (distance < detectionRange || aggroed)
        {
            aggroed = true;

            HandleMovement();
            TryShoot();
        }
        else
        {
            moveDirection = 0f;
            animator.SetBool("isMoving", false);
        }

        Flip();
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        velocity.x = moveDirection * moveSpeed;

        rb.linearVelocity = velocity;
    }

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
            if (HasClearShot() && IsHeightValid())
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

    bool HasClearShot()
    {
        Vector2 direction = (player.position - firePoint.position).normalized;
        float dist = Vector2.Distance(firePoint.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            dist,
            groundLayer | playerLayer
        );

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    bool IsHeightValid()
    {
        float heightDiff = Mathf.Abs(player.position.y - firePoint.position.y);
        return heightDiff < 1.5f; // tweak this if needed
    }

    void Flip()
    {
        float direction = player.position.x - transform.position.x;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(direction) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void OnDamaged()
    {
        aggroed = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }
}