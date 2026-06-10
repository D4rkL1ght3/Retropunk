using UnityEngine;

[System.Serializable]
public class WeaponStatRating
{
    public string statName;

    [Range(0f, 10f)]
    public int rating = 5;

    public Sprite statIcon;
}