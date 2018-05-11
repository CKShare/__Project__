using System.Collections.Generic;

public enum PhysiqueType
{
    Normal,
    Huge
}

public class PhysiqueTypeComparer : IEqualityComparer<PhysiqueType>
{
    public bool Equals(PhysiqueType x, PhysiqueType y)
    {
        return x == y;
    }

    public int GetHashCode(PhysiqueType obj)
    {
        return obj.GetHashCode();
    }
}