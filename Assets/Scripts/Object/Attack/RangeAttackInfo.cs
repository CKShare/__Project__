using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class RangeAttackInfo : SerializedScriptableObject
{
    [SerializeField]
    private string _proejctilePool;
    [SerializeField, FMODUnity.EventRef]
    private string _fireSound;
    [SerializeField]
    private float _fireForce;

    public string ProjectilePool => _proejctilePool;
    public string FireSound => _fireSound;
    public float FireForce => _fireForce;
}
