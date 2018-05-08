using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu]
public class PlayerDatabase : CharacterDatabase
{
    [SerializeField, Tooltip("초당 체력 회복 단위")]
    private float _healthRegenUnit = 5F;
    [SerializeField, Tooltip("이동 속도")]
    private float _moveSpeedMultiplier = 10F;
    [SerializeField, Tooltip("현재 이동 속도에서 목표 이동 속도로 얼마나 빨리 전환되는지에 대한 값")]
    private float _moveSpeedLerpMultiplier = 10F;
    [SerializeField, Tooltip("현재 각도에서 목표 각도로 얼마나 빨리 전환되는지에 대한 값 (회전 속도)")]
    private float _moveRotateLerpMultiplier = 10F;


    public float HealthRegenUnit => _healthRegenUnit;
    public float MoveSpeedMultiplier => _moveSpeedMultiplier;
    public float MoveSpeedLerpMultiplier => _moveSpeedLerpMultiplier;
    public float MoveRotateLerpMultiplier => _moveRotateLerpMultiplier;
}
