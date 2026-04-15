using UnityEngine;

public class EnemyRanged : MonoBehaviour, IEntity
{
    public Transform player;

    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
    public float shootRange = 5f;
    public float retreatRange = 3f;

    public float shootCooldown = 1.5f;
    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform firePoint;

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
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);
        }

        Flip();
    }

    void HandleMovement()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        if (distance > shootRange)
        {
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            animator.SetBool("isMoving", true);
        }
        else if (distance < retreatRange)
        {
            rb.linearVelocity = new Vector2(-direction * moveSpeed, rb.linearVelocity.y);
            animator.SetBool("isMoving", true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
        }
    }

    void TryShoot()
    {
        if (distance <= shootRange && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        animator.SetTrigger("Shoot");

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        EnemyProjectile p = projectile.GetComponent<EnemyProjectile>();
        if (p != null)
        {
            p.Initialize(direction);
        }
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