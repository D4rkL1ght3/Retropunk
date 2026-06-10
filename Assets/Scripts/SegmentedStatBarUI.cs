using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentedStatBarUI : MonoBehaviour
{
    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private Image statIconImage;

    [Header("Segments")]
    public List<Image> segments = new List<Image>();

    [Header("Stat Settings")]
    public float ratingPerSegment = 1f;

    public void SetStat(WeaponStatRating stat)
    {
        if (stat == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (statNameText != null)
            statNameText.text = stat.statName;

        if (statIconImage != null)
        {
            statIconImage.sprite = stat.statIcon;
            statIconImage.gameObject.SetActive(stat.statIcon != null);
        }

        UpdateStatBar(stat.rating);
    }

    void UpdateStatBar(float currentRating)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            float segmentStart = i * ratingPerSegment;

            float fill = (currentRating - segmentStart) / ratingPerSegment;

            fill = Mathf.Clamp01(fill);

            segments[i].fillAmount = fill;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].fillAmount = 0f;
        }

        gameObject.SetActive(false);
    }
}