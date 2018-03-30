using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class PersistentAttribute : Attribute
{

}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _isDestroyed = false;

    protected virtual void Awake()
    {
        // Check whether this gameObject is persistent or not.
        var attrs = typeof(T).GetCustomAttributes(typeof(PersistentAttribute), true);
        if (attrs.Length > 0)
            DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        _isDestroyed = true;
    }

    public static T Instance
    {
        get
        {
            if (_isDestroyed)
                return null;

            return Nested._instance;
        }
    }

    private class Nested
    {
        internal readonly static T _instance = null;

        static Nested()
        {
            T[] typeRef = FindObjectsOfType<T>();

            if (typeRef.Length == 0) // No Instances in the hirarchy.
            {
                GameObject singleton = new GameObject($"(Singleton) {typeof(T)}");
                _instance = singleton.AddComponent<T>();
            }
            else if (typeRef.Length == 1) // Only one instance in the hirarchy.
            {
                _instance = typeRef[0];
            }
            else if (typeRef.Length > 1) // Instances more than one in the hirarchy.
            {
                throw new UnityException($"'{typeof(T)}' Type has instances more than one in the hirarchy!");
            }
        }
    }
}