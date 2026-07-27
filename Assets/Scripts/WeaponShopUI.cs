using TMPro;
using UnityEngine;

public class WeaponShopUI : MonoBehaviour
{
    public enum ShopCategory
    {
        Primary,
        Secondary,
        Melee
    }

    [Header("Database")]
    [SerializeField] private WeaponDatabase database;

    [Header("Navigation")]
    [SerializeField] private GameObject loadoutPanel;

    [Header("Scroll View")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private WeaponShopItemUI itemPrefab;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private TextMeshProUGUI categoryText;

    private ShopCategory currentCategory = ShopCategory.Primary;

    void OnEnable()
    {
        RefreshShop();
    }

    public void ShowPrimary()
    {
        currentCategory = ShopCategory.Primary;
        RefreshShop();
    }

    public void ShowSecondary()
    {
        currentCategory = ShopCategory.Secondary;
        RefreshShop();
    }

    public void ShowMelee()
    {
        currentCategory = ShopCategory.Melee;
        RefreshShop();
    }

    public void RefreshShop()
    {
        UpdateCashUI();
        UpdateCategoryText();
        ClearItems();
        BuildItems();
    }

    void UpdateCashUI()
    {
        if (cashText == null) return;

        int cash = CashWallet.Instance != null
            ? CashWallet.Instance.CurrentCash
            : 0;

        cashText.text = "$" + cash;
    }

    void UpdateCategoryText()
    {
        if (categoryText != null)
            categoryText.text = currentCategory.ToString();
    }

    void ClearItems()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    void BuildItems()
    {
        if (database == null || itemPrefab == null || contentParent == null)
        {
            Debug.LogWarning("WeaponShopUI is missing database, item prefab, or content parent!");
            return;
        }

        switch (currentCategory)
        {
            case ShopCategory.Primary:
                foreach (GunData gun in database.primaryGuns)
                {
                    if (gun == null) continue;

                    WeaponShopItemUI item = Instantiate(itemPrefab, contentParent);
                    item.SetupGun(gun, this);
                }
                break;

            case ShopCategory.Secondary:
                foreach (GunData gun in database.secondaryGuns)
                {
                    if (gun == null) continue;

                    WeaponShopItemUI item = Instantiate(itemPrefab, contentParent);
                    item.SetupGun(gun, this);
                }
                break;

            case ShopCategory.Melee:
                foreach (MeleeData melee in database.meleeWeapons)
                {
                    if (melee == null) continue;

                    WeaponShopItemUI item = Instantiate(itemPrefab, contentParent);
                    item.SetupMelee(melee, this);
                }
                break;
        }
    }

    public void ExitShop()
    {
        gameObject.SetActive(false);

        if (loadoutPanel != null)
            loadoutPanel.SetActive(true);
    }
}