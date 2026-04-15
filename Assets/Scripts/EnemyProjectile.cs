using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 1f;
    public int damage = 4;

    public LayerMask groundLayer;
    Vector2 moveDirection;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
        }

        PlayerHealth health = collision.GetComponent<PlayerHealth>();

        if (health != null && health.enabled == true)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }
}