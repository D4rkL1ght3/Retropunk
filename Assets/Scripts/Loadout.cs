[System.Serializable]
public class Loadout
{
    public Gun primary;
    public Gun secondary;
    public MeleeWeapon melee;

    public Gun GetGun(LoadoutSlot slot)
    {
        switch (slot)
        {
            case LoadoutSlot.Primary: return primary;
            case LoadoutSlot.Secondary: return secondary;
            default: return null;
        }
    }

    public MeleeWeapon GetMelee()
    {
        return melee;
    }

    public void SetGun(LoadoutSlot slot, GunData data)
    {
        Gun newGun = new Gun(data);

        switch (slot)
        {
            case LoadoutSlot.Primary: primary = newGun; break;
            case LoadoutSlot.Secondary: secondary = newGun; break;
        }
    }

    public void SetMelee(MeleeData data)
    {
        melee = new MeleeWeapon(data);
    }
}