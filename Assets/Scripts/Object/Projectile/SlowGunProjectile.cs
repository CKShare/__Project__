using UnityEngine;

public class SlowGunProjectile : Projectile
{
    [SerializeField]
    private GameObject _slowArea;
    [SerializeField]
    private AnimationCurve _slowCurve;
    [SerializeField]
    private float _duration;
    [SerializeField]
    private float _applyDelay;

    protected override void OnCollideWith(Transform target)
    {
        base.OnCollideWith(target);
        
        _slowArea.GetComponent<SlowArea>().Set(_applyDelay, _duration, _slowCurve);
        _slowArea.SetActive(true);
        gameObject.SetActive(false);
    }
}