using UnityEngine;
using System.Collections;

public class SeekingMissile : MonoBehaviour, IEntity
{
    [Header("Target")]
    public Transform player;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float turnSpeed = 180f;
    public float lifetime = 4f;

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

        StartCoroutine(DestroyAfterLifetime());
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            return;

        if (((1 << collision.gameObject.layer) & (groundLayer | playerLayer)) != 0)
        {
            Explode();
        }
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

    IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }

    public void OnDamaged()
    {
        Explode();
    }
    public void Disable() { }
    public void OnDeath() { }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}