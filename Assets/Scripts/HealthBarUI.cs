using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public PlayerController playerController;

    public List<Image> segments = new List<Image>();
    public int healthPerSegment = 4;
    public float flashDuration = 0.2f;

    public Image healCooldown;
    public Button healButton;

    void Start()
    {
        playerHealth.OnDamaged += DamageFlashSegments;
        playerHealth.OnHealed += HealFlashSegments;
    }

    void Update()
    {
        UpdateHealthBar();
        UpdateHealCooldown();
    }

    void UpdateHealthBar()
    {
        int currentHealth = playerHealth.CurrentHealth;

        for (int i = 0; i < segments.Count; i++)
        {
            float segmentStartHealth = i * healthPerSegment;

            float fill = (currentHealth - segmentStartHealth) / healthPerSegment;

            fill = Mathf.Clamp01(fill);

            segments[i].fillAmount = fill;
        }
    }

    void UpdateHealCooldown()
    {
        float cooldownTime = playerController.HealCooldownTimer;
        float cooldownDuration = playerController.HealCooldown;

        float fill = cooldownTime / cooldownDuration;
        fill = Mathf.Clamp01(fill);
        healCooldown.fillAmount = fill;

        if (cooldownTime > 0)
        {
            healButton.interactable = false;
        }
        else
        {
            healButton.interactable = true;
        }
    }

    public void Heal()
    {
        playerController.TryStartHealing();
    }

    void DamageFlashSegments()
    {
        StopAllCoroutines();
        StartCoroutine(DamageFlashCoroutine());
    }
    void HealFlashSegments()
    {
        StopAllCoroutines();
        StartCoroutine(HealFlashCoroutine());
    }

    IEnumerator DamageFlashCoroutine()
    {
        foreach (var img in segments)
        {
            img.color = Color.yellow;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var img in segments)
        {
            img.color = Color.white;
        }
    }

    IEnumerator HealFlashCoroutine()
    {
        foreach (var img in segments)
        {
            img.color = Color.lightGreen;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var img in segments)
        {
            img.color = Color.white;
        }
    }
}