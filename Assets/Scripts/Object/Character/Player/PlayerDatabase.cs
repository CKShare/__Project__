using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class PlayerDatabase : CharacterDatabase
{
    [SerializeField]
    private float _healthRegenUnit;
    [SerializeField]
    private AnimationCurve _moveCurve;
    [SerializeField]
    private float _moveLerpTime;
    [SerializeField]
    private float _rotateSpeed;
    [SerializeField]
    private AnimationCurve _dashCurve;
    [SerializeField]
    private float _dashCoolTime;
    [SerializeField]
    private LayerMask _targetLayer;
    [SerializeField]
    private float _targetingMaxDistance;
    [SerializeField]
    private float _targetingMaxAngle;
    [SerializeField]
    private int[] _maxCombos = new int[0];

    [SerializeField]
    private string _inputHorizontal = "Horizontal";
    [SerializeField]
    private string _inputVertical = "Vertical";
    [SerializeField]
    private string _inputAttack = "Attack";
    [SerializeField]
    private string _inputDash = "Dash";
    [SerializeField]
    private string _inputAim = "Aim";
    
    public float HealthRegenUnit => _healthRegenUnit;
    public AnimationCurve MoveCurve => _moveCurve;
    public float MoveLerpTime => _moveLerpTime;
    public float RotateSpeed => _rotateSpeed;
    public AnimationCurve DashCurve => _dashCurve;
    public float DashCoolTime => _dashCoolTime;
    public LayerMask TargetLayer => _targetLayer;
    public float TargetingMaxDistance => _targetingMaxDistance;
    public float TargetingMaxAngle => _targetingMaxAngle;
    public int[] MaxCombos => _maxCombos;
    public string InputHorizontal => _inputHorizontal;
    public string InputVertical => _inputVertical;
    public string InputDash => _inputDash;
    public string InputAttack => _inputAttack;
    public string InputAim => _inputAim;
}