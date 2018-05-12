using UnityEngine;

public class ComboPattern_Front : ComboPattern
{
    public override bool CheckTransition(PlayerController controller)
    {
        return true;        
    }
}