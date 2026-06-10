using UnityEngine;

public class WeaponStatsPanelUI : MonoBehaviour
{
    [SerializeField] private SegmentedStatBarUI[] statBars;

    public void DisplayStats(WeaponStatRating[] stats)
    {
        for (int i = 0; i < statBars.Length; i++)
        {
            if (stats != null && i < stats.Length)
            {
                statBars[i].SetStat(stats[i]);
            }
            else
            {
                statBars[i].Clear();
            }
        }
    }
}