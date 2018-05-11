using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public abstract class CastHitInfo
{
    [SerializeField, Required]
    private string _pointName;
    [SerializeField]
    private int _damage;
    [SerializeField, DisableContextMenu]
    private Dictionary<PhysiqueType, ForceInfo> _forceDict = new Dictionary<PhysiqueType, ForceInfo>(new PhysiqueTypeComparer());
    [SerializeField]
    private EffectSettings _hitEffect;

    public string PointName => _pointName;
    public int Damage => _damage;
    public bool TryGetForceInfo(PhysiqueType physiqueType, out ForceInfo forceInfo) => _forceDict.TryGetValue(physiqueType, out forceInfo);
    public EffectSettings HitEffect => _hitEffect;
}