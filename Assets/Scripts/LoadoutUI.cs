using UnityEngine;

public class LoadoutUI : MonoBehaviour
{
    private Loadout tempLoadout = new Loadout();

    void Start()
    {
        Time.timeScale = 0f;
    }

    public void SelectPrimary(GunData data)
    {
        tempLoadout.SetGun(LoadoutSlot.Primary, data);
    }

    public void SelectSecondary(GunData data)
    {
        tempLoadout.SetGun(LoadoutSlot.Secondary, data);
    }

    public void SelectMelee(MeleeData data)
    {
        tempLoadout.SetMelee(data);
    }

    public void Confirm()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        player.SetLoadout(tempLoadout);

        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}