using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerDatabase : CharacterDatabase
{
    [SerializeField]
    private float _healthRegenUnit = 5F;
    [SerializeField]
    private float _moveSpeedMultiplier = 10F;
    [SerializeField]
    private float _moveSpeedLerpMultiplier = 10F;
    [SerializeField]
    private float _moveRotateLerpMultiplier = 10F;

    public float HealthRegenUnit => _healthRegenUnit;
    public float MoveSpeedMultiplier => _moveSpeedMultiplier;
    public float MoveSpeedLerpMultiplier => _moveSpeedLerpMultiplier;
    public float MoveRotateLerpMultiplier => _moveRotateLerpMultiplier;
}

public struct AttackInfo
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;

    public int Damage => _damage;
    public int ReactionID => _reactionID;
}