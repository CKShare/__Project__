using UnityEngine;
using Pathfinding;

public class TimeControlRichAI : ITimeControl
{
    [SerializeField]
    private RichAI _richAI;
    
    public void AdjustTimeScale(float ratio)
    {
        _richAI.maxSpeed *= ratio;
        _richAI.rotationSpeed *= ratio;
        _richAI.gravity.y *= ratio;
    }
}