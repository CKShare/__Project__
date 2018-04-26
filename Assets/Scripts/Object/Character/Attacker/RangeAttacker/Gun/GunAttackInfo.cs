using UnityEngine;

[CreateAssetMenu]
public class GunAttackInfo : RangeAttackInfo
{
    [SerializeField]
    private Vector2 _horizontalError;
    [SerializeField]
    private Vector2 _verticalError;

    public float HorizontalError => Random.Range(_horizontalError.x, _horizontalError.y);
    public float VerticalError => Random.Range(_verticalError.x, _verticalError.y);
}