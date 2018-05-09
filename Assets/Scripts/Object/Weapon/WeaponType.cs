using System.Collections.Generic;

public enum WeaponType
{
    Primary,
    Secondary,
    Melee
}

public class WeaponTypeComparer : IEqualityComparer<WeaponType>
{
    public bool Equals(WeaponType x, WeaponType y)
    {
        return x == y;
    }

    public int GetHashCode(WeaponType obj)
    {
        return obj.GetHashCode();
    }
}
