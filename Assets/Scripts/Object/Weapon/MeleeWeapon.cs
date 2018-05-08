using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected class MeleeHitInfo : HitInfo
    {
        [SerializeField]
        private Transform _hitPoint;
        public Transform HitPoint => _hitPoint;
    }

    [SerializeField]
    private Dictionary<int, MeleeHitInfo> _hitDict = new Dictionary<int, MeleeHitInfo>();

    public void CheckHit(int attackCode)
    {
        MeleeHitInfo hitInfo;
        if (_hitDict.TryGetValue(attackCode, out hitInfo))
        {

        }
    }
}