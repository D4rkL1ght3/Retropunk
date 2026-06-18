using UnityEngine;

public class LoadoutUI : MonoBehaviour
{
    public WeaponSelectionUI selectionUI;
    public LoadoutSlotUI[] slots;

    void OnEnable()
    {
        // Refresh the loadout slots when the UI is enabled
        var slots = GetComponentsInChildren<LoadoutSlotUI>();
        foreach (var slot in slots)
        {
            slot.Refresh();
        }
    }

    public void OpenPrimary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Primary);
        gameObject.SetActive(false);
    }

    public void OpenSecondary()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Secondary);
        gameObject.SetActive(false);
    }

    public void OpenMelee()
    {
        selectionUI.Open(WeaponSelectionUI.SlotType.Melee);
        gameObject.SetActive(false);
    }
}