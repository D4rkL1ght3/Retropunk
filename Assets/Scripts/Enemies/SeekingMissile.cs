using UnityEngine;

public class SeekingMissile : MonoBehaviour, IEntity
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float turnSpeed = 90f;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionRadius = 1.5f;
    public int explosionDamage = 8;

    private bool exploded = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    void Update()
    {
        if (exploded || player == null)
            return;

        SeekPlayer();
    }

    void SeekPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float currentAngle = transform.eulerAngles.z;

        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            turnSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            newAngle
        );

        transform.position += transform.right * moveSpeed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Explode();
    }

    public void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        DealExplosionDamage();

        Destroy(gameObject);
    }

    void DealExplosionDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(explosionDamage);
        }
    }

    public void OnDamaged() { }

    public void Disable() { }

    public void OnDeath()
    {
        Explode();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}