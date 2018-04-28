using System;
using UnityEngine;

[Serializable]
public struct HitInfo
{
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _reactionID;

    public int Damage => _damage;
    public int ReactionID => _reactionID;
}