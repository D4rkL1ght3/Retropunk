using UnityEngine;

public class LoadoutUI : MonoBehaviour
{
    public WeaponSelectionUI selectionUI;

    public void OpenPrimary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Primary);
    }

    public void OpenSecondary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Secondary);
    }

    public void OpenMelee()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Melee);
    }
}