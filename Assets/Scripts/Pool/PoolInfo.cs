using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct PoolInfo
{
    [SerializeField, Required]
    private string _poolName;

    private Pool<GameObject> _pool;
#if UNITY_EDITOR
    private List<string> PoolNames => PoolManager.Instance.PreloadPoolNames;
#endif

    public Pool<GameObject> Pool => _pool == null ? (_pool = PoolManager.Instance[_poolName]) : _pool;
}