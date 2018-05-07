using UnityEngine;

[CreateAssetMenu]
public class InputSettings : ScriptableObject
{
    [SerializeField]
    private string _horizontalAxisName = "Horizontal";
    [SerializeField]
    private string _verticalAxisName = "Vertical";
    [SerializeField]
    private string _attackButtonName = "Attack";

    public string HorizontalAxisName => _horizontalAxisName;
    public string VerticalAxisName => _verticalAxisName;
    public string AttackButtonName => _attackButtonName;
}