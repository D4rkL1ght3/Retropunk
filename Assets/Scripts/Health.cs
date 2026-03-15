using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public int maxHealth = 40;
    private int currentHealth;

    private Animator[] animators;
    private SpriteRenderer[] spriteRenderers;
    private Rigidbody2D rb;

    bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animators = GetComponentsInChildren<Animator>(true);
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

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

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (CompareTag("Player"))
        {
            GameObject armPivot = transform.Find("ArmPivot").gameObject;

            if (armPivot != null)
            {
                armPivot.SetActive(false);
            }
        }

        foreach (Animator anim in animators)
        {
            anim.SetTrigger("Death");
        }

        IEntity controller = GetComponent<IEntity>();
        if (controller != null)
        {
            controller.Disable();
        }
    }
}