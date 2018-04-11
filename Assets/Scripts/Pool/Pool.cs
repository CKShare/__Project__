using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Pool<T> where T : class
{
    public delegate T ObjectCreator();
    public delegate int ExtendCapacityProvider(int capacity);
    public delegate void SpawnCallback(T poolObject);
    public delegate void DespawnCallback(T poolObject);

    private readonly ObjectCreator _objectCreator;
    private readonly ExtendCapacityProvider _extendCapacityProvider;
    private readonly SpawnCallback _spawnCallback;
    private readonly DespawnCallback _despawnCallback;
    private readonly Stack<T> _pool;
    
    private readonly int _InitialCapacity = 0;

    public Pool(int capacity, ObjectCreator objectCreator) : this(capacity, objectCreator, null, null, null) { }
    public Pool(int capacity, ObjectCreator objectCreator, SpawnCallback spawnCallback) : this(capacity, objectCreator, null, spawnCallback, null) { }
    public Pool(int capacity, ObjectCreator objectCreator, ExtendCapacityProvider extendCapacityProvider) : this(capacity, objectCreator, extendCapacityProvider, null, null) { }
    public Pool(int capacity, ObjectCreator objectCreator, ExtendCapacityProvider extendCapacityProvider, SpawnCallback spawnCallback) : this(capacity, objectCreator, extendCapacityProvider, spawnCallback, null) { }
    public Pool(int capacity, ObjectCreator objectCreator, DespawnCallback despawnCallback) : this(capacity, objectCreator, null, null, despawnCallback) { }
    public Pool(int capacity, ObjectCreator objectCreator, ExtendCapacityProvider extendCapacityProvider, DespawnCallback despawnCallback) : this(capacity, objectCreator, extendCapacityProvider, null, despawnCallback) { }
    public Pool(int capacity, ObjectCreator objectCreator, SpawnCallback spawnCallback, DespawnCallback despawnCallback) : this(capacity, objectCreator, null, spawnCallback, despawnCallback) { }
    public Pool(int capacity, ObjectCreator objectCreator, ExtendCapacityProvider extendCapacityProvider, SpawnCallback spawnCallback, DespawnCallback despawnCallback)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("capacity");
        if (objectCreator == null)
            throw new ArgumentNullException("objectCreator");

        _InitialCapacity = capacity;
        _objectCreator = objectCreator;
        _extendCapacityProvider = extendCapacityProvider ?? (x => 1);
        _spawnCallback = spawnCallback;
        _despawnCallback = despawnCallback;
        _pool = new Stack<T>(_InitialCapacity);

        ExtendPool();
    }

    //[MethodImpl(MethodImplOptions.Synchronized)]
    public T Spawn()
    {
        if (_pool.Count <= 0)
            ExtendPool();

        T poolObject = _pool.Pop();
        _spawnCallback?.Invoke(poolObject);

        return poolObject;
    }

    public void Despawn(T poolObject)
    {
        _despawnCallback?.Invoke(poolObject);
        _pool.Push(poolObject);
    }

    private void ExtendPool()
    {
        int extendCapacity;
        if (Capacity < _InitialCapacity)
        {
            extendCapacity = _InitialCapacity;
        }
        else
        {
            extendCapacity = _extendCapacityProvider(Capacity);
            if (extendCapacity <= 0)
                extendCapacity = 1;
        }

        for (int i = 0; i < extendCapacity; i++)
        {
            _pool.Push(_objectCreator());
        }

        Capacity += extendCapacity;
    }

    public int Count
    {
        get { return _pool.Count; }
    }

    public int Capacity
    {
        get;
        private set;
    }
}