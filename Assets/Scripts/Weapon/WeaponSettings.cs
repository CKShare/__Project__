using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class WeaponSettings : ScriptableObject
{
    [SerializeField]
    private int _weaponType = 0;
    [SerializeField]
    private int _baseDamage = 0;
    [SerializeField, BoxGroup("Collision Bounds")]
    private Vector3 _boundCenter = Vector3.zero;
    [SerializeField, BoxGroup("Collision Bounds")]
    private Vector3 _boundSize = Vector3.one;
    
    public int WeaponType => _weaponType;
    public int BaseDamage => _baseDamage;
    public Vector3 BoundCenter => _boundCenter;
    public Vector3 BoundSize => _boundSize;
}

