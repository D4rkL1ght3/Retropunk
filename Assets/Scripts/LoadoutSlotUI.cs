using UnityEngine;
using UnityEngine.UI;

public class LoadoutSlotUI : MonoBehaviour
{
    public enum SlotType
    {
        Primary,
        Secondary,
        Melee
    }

    public SlotType slotType;
    public Image weaponIcon;

    public void Refresh()
    {
        var loadout = LoadoutManager.Instance.currentLoadout;

        switch (slotType)
        {
            case SlotType.Primary:
                weaponIcon.sprite = loadout.primaryGun != null
                    ? loadout.primaryGun.baseSprite
                    : null;
                break;

            case SlotType.Secondary:
                weaponIcon.sprite = loadout.secondaryGun != null
                    ? loadout.secondaryGun.baseSprite
                    : null;
                break;

            case SlotType.Melee:
                weaponIcon.sprite = loadout.meleeWeapon != null
                    ? loadout.meleeWeapon.icon
                    : null;
                break;
        }
    }
}