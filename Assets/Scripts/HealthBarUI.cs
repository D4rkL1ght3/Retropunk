using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public List<Image> segments = new List<Image>();

    public int healthPerSegment = 4;
    public float flashDuration = 0.2f;

    void Start()
    {
        playerHealth.OnDamaged += DamageFlashSegments;
        playerHealth.OnHealed += HealFlashSegments;
    }

    void Update()
    {
        UpdateHealthBar();
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
            img.color = Color.green;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var img in segments)
        {
            img.color = Color.white;
        }
    }
}