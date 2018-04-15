using UnityEngine;
using Sirenix.OdinInspector;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [SerializeField, MinValue(1)]
    private int _maxHit;
    [SerializeField]
    private GameObject _destructedObject;

    private int _currentHit;

    private void Awake()
    {
        _currentHit = _maxHit;
    }

    public void ApplyDamage(Transform attacker, int damage, int reactionID = -1)
    {
        if (_currentHit > 0)
        {
            _currentHit--;
            if (_currentHit <= 0)
                OnDestructed();
        }
    }

    private void OnDestructed()
    {
        _destructedObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
