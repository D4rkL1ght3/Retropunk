using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StaminaBarUI : MonoBehaviour
{
    public PlayerController player;

    public List<Image> segments = new List<Image>();

    public float staminaPerSegment = 1f;

    void Update()
    {
        UpdateStaminaBar();
    }

    void UpdateStaminaBar()
    {
        float currentStamina = player.CurrentStamina;

        for (int i = 0; i < segments.Count; i++)
        {
            float segmentStart = i * staminaPerSegment;

            float fill = (currentStamina - segmentStart) / staminaPerSegment;

            fill = Mathf.Clamp01(fill);

            segments[i].fillAmount = fill;

            if (currentStamina < 1f)
            {
                segments[i].color = Color.red;
            }
            else
            {
                segments[i].color = Color.white;
            }
        }
    }
}