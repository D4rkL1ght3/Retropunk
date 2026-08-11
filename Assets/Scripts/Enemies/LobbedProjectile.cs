using UnityEngine;

public class LobbedProjectile : MonoBehaviour
{
    public float lifetime = 3f;
    public int damage = 8;

    public LayerMask groundLayer;
    public bool rotateToVelocity = true;

    private Rigidbody2D rb;

    public void Initialize(Vector2 velocity)
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = velocity;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rotateToVelocity && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(
                rb.linearVelocity.y,
                rb.linearVelocity.x
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            return;

        PlayerHealth health = collision.GetComponent<PlayerHealth>();

        if (health != null && health.enabled)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}