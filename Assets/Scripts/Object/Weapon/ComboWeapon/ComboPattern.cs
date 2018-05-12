using UnityEngine;

public abstract class ComboPattern
{
    [SerializeField]
    private int _patternID;
    [SerializeField]
    private int _priority;
    [SerializeField]
    private int _maxCombo;

    public abstract bool CheckTransition(PlayerController controller);
    public int PatternID => _patternID;
    public int Priority => _priority;
    public int MaxCombo => _maxCombo;
}