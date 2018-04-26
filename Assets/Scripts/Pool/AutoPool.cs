using UnityEngine;

public class AutoPool : MonoBehaviour
{
    [SerializeField]
    private string _poolName;
    [SerializeField]
    private float _duration;

    private float _elapsedTime;

    private void OnEnable()
    {
        _elapsedTime = 0F;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > _duration)
            PoolManager.Instance[_poolName].Despawn(gameObject);
    }
}