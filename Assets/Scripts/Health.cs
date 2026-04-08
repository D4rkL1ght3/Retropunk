using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public int maxHealth = 40;
    private int currentHealth;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private IEntity controller;

    bool isDead = false;
    public Color damagedColor = Color.red;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>(true);
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        controller = GetComponent<IEntity>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (controller != null)
            controller.OnDamaged();

        StopAllCoroutines();
        StartCoroutine(DamageFlash());

        Debug.Log(gameObject.name + " took damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        spriteRenderer.color = damagedColor;

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (controller != null)
            controller.Disable();

        animator.SetTrigger("Death");
    }
}