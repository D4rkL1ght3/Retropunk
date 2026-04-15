using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 40;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public System.Action OnDamaged;
    public System.Action OnHealed;

    private Animator animator;
    private SpriteRenderer[] spriteRenderers;
    private Rigidbody2D rb;
    private PlayerController playerController;

    bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = transform.Find("DefaultModel").GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        StopAllCoroutines();
        StartCoroutine(DamageFlash());
        OnDamaged?.Invoke();

        Debug.Log(gameObject.name + " took damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int heal)
    {
        currentHealth += heal;

        StopAllCoroutines();
        StartCoroutine(HealFlash());
        OnHealed?.Invoke();

        Debug.Log(gameObject.name + " healed for " + heal);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    IEnumerator DamageFlash()
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = Color.red;
        }

        yield return new WaitForSeconds(0.2f);

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = Color.white;
        }
    }

    IEnumerator HealFlash()
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = Color.green;
        }
        yield return new WaitForSeconds(0.2f);
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = Color.white;
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        playerController.EnterDefaultMode();

        if (playerController != null)
            playerController.enabled = false;

        animator.SetTrigger("Death");
    }
}