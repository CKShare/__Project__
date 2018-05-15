using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-32000)]
public class PoolManager : MonoSingleton<PoolManager>
{
    [Serializable]
    private struct PreloadInfo
    {
        [SerializeField]
        private Transform _parent;
        [SerializeField, Required]
        private GameObject _prefab;
        [SerializeField]
        private int _initialCapacity;
        [SerializeField, MinValue(1)]
        private int _extendCapacity;

        public Transform Parent => _parent;
        public GameObject Prefab => _prefab;
        public int InitialCapacity => _initialCapacity;
        public int ExtendCapacity => _extendCapacity;
    }
    
    [SerializeField, DisableInPlayMode]
    private Dictionary<string, PreloadInfo> _preloadDict = new Dictionary<string, PreloadInfo>();
    private Dictionary<string, Pool<GameObject>> _poolDict = new Dictionary<string, Pool<GameObject>>();

    protected override void Awake()
    {
        base.Awake();
        
        CreatePools();
    }

    private void CreatePools()
    {
        foreach (var preload in _preloadDict)
        {
            Pool<GameObject> newPool = new Pool<GameObject>(preload.Value.InitialCapacity,
            () =>
            {
                Transform container = null;
                if (preload.Value.Parent == null)
                {
                    container = FindContainer(preload.Key);
                    if (container == null)
                    {
                        container = new GameObject(preload.Key).transform;
                        container.parent = transform;
                    }
                }
                else
                {
                    container = preload.Value.Parent;
                }

                GameObject instance = Instantiate(preload.Value.Prefab, container);
                instance.SetActive(false);

                return instance;
            },
            _ => preload.Value.ExtendCapacity,
            obj =>
            {
                obj.SetActive(true);
            },
            obj =>
            {
                obj.SetActive(false);
            });

            _poolDict.Add(preload.Key, newPool);
        }
    }

    private Transform FindContainer(string poolName)
    {
        foreach (Transform child in transform)
        {
            if (string.CompareOrdinal(child.name, poolName) == 0)
                return child;
        }

        return null;
    }

    public Pool<GameObject> GetPool(string poolName)
    {
        Pool<GameObject> pool;
        if (!_poolDict.TryGetValue(poolName, out pool))
            throw new KeyNotFoundException(poolName);

        return pool;    
    }

    public Pool<GameObject> this[string poolName]
    {
        get
        {
            return GetPool(poolName);
        }
    }

#if UNITY_EDITOR
    public List<string> PreloadPoolNames => new List<string>(_preloadDict.Keys);
#endif
}