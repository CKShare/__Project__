using UnityEngine;
using Sirenix.OdinInspector;

public abstract class RangeAttacker
{
    [SerializeField, Required]
    private Transform _muzzle;
    [SerializeField, Required]
    private ParticleSystem _muzzleEffect;

    public virtual void Attack(Transform attacker, Vector3 targetPosition)
    {
        _muzzleEffect.Play(true);
        FMODUnity.RuntimeManager.PlayOneShot(AttackInfo.FireSound, _muzzle.position);
    }

    protected Vector3 MuzzlePosition => _muzzle.position;
    protected RangeAttackInfo AttackInfo { get; set; }
}