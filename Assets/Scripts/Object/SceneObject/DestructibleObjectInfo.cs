using UnityEngine;

[CreateAssetMenu]
public class DestructibleObjectInfo : ScriptableObject
{
    [SerializeField]
    private int _maxHit;
    [SerializeField]
    private EffectInfo _destructEffectInfo;

    public int MaxHit => _maxHit;
    public EffectInfo DestructEffectInfo => _destructEffectInfo;
}