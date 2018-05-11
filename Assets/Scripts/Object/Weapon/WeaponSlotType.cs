using System.Collections.Generic;

public enum WeaponSlotType
{
    Primary,
    Secondary,
    Melee
}

public class WeaponSlotTypeComparer : IEqualityComparer<WeaponSlotType>
{
    public bool Equals(WeaponSlotType x, WeaponSlotType y)
    {
        return x == y;
    }

    public int GetHashCode(WeaponSlotType obj)
    {
        return obj.GetHashCode();
    }
}
