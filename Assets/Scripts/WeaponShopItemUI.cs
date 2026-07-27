using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Button buyButton;

    [Header("Optional Stats")]
    [SerializeField] private WeaponStatsPanelUI statsPanel;

    private GunData gunData;
    private MeleeData meleeData;
    private WeaponShopUI shopUI;

    public void SetupGun(GunData gun, WeaponShopUI ownerShop)
    {
        gunData = gun;
        meleeData = null;
        shopUI = ownerShop;

        Refresh();
    }

    public void SetupMelee(MeleeData melee, WeaponShopUI ownerShop)
    {
        meleeData = melee;
        gunData = null;
        shopUI = ownerShop;

        Refresh();
    }

    void Refresh()
    {
        bool isGun = gunData != null;

        string weaponName = isGun ? gunData.gunName : meleeData.weaponName;
        Sprite icon = isGun ? gunData.baseSprite : meleeData.icon;
        int price = isGun ? gunData.price : meleeData.price;
        bool owned = IsOwned();

        if (weaponNameText != null)
            weaponNameText.text = weaponName;

        if (weaponIcon != null)
            weaponIcon.sprite = icon;

        if (statsPanel != null)
        {
            if (isGun)
                statsPanel.DisplayStats(gunData.statRatings);
            else
                statsPanel.DisplayStats(meleeData.statRatings);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();

            buyButton.interactable = !owned && CanAfford(price);

            buyButton.onClick.AddListener(Buy);

            TextMeshProUGUI buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = owned ? "Owned" : "$" + price;
        }
    }

    bool IsOwned()
    {
        if (WeaponOwnershipManager.Instance == null)
            return false;

        if (gunData != null)
            return WeaponOwnershipManager.Instance.IsGunOwned(gunData);

        if (meleeData != null)
            return WeaponOwnershipManager.Instance.IsMeleeOwned(meleeData);

        return false;
    }

    bool CanAfford(int price)
    {
        if (CashWallet.Instance == null)
            return false;

        return CashWallet.Instance.CanAfford(price);
    }

    void Buy()
    {
        bool bought = false;

        if (WeaponOwnershipManager.Instance == null)
        {
            Debug.LogWarning("WeaponOwnershipManager not found!");
            return;
        }

        if (gunData != null)
            bought = WeaponOwnershipManager.Instance.BuyGun(gunData);
        else if (meleeData != null)
            bought = WeaponOwnershipManager.Instance.BuyMelee(meleeData);

        if (bought)
        {
            Debug.Log("Bought weapon!");

            if (shopUI != null)
                shopUI.RefreshShop();
        }
    }
}