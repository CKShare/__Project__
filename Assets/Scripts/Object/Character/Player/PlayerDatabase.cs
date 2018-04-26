using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class PlayerDatabase : CharacterDatabase
{
    [SerializeField]
    private float _healthRegenUnit;
    [SerializeField]
    private float _dashCoolTime;

    [SerializeField]
    private string _inputHorizontal = "Horizontal";
    [SerializeField]
    private string _inputVertical = "Vertical";
    [SerializeField]
    private string _inputAttack = "Attack";
    [SerializeField]
    private string _inputDash = "Dash";
    
    public float HealthRegenUnit => _healthRegenUnit;
    public float DashCoolTime => _dashCoolTime;
    public string InputHorizontal => _inputHorizontal;
    public string InputVertical => _inputVertical;
    public string InputDash => _inputDash;
    public string InputAttack => _inputAttack;
}