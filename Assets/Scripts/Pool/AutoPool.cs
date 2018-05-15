using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class AutoPool : MonoBehaviour
{
    private enum PoolType
    {
        Duration,
        Disabled
    }

    [SerializeField, HideLabel]
    private PoolInfo _poolInfo;
    [SerializeField]
    private PoolType _poolType = PoolType.Duration;
    [SerializeField, ShowIf("_poolType", optionalValue:PoolType.Duration)]
    private float _duration;

    private void OnEnable()
    {
        if (_poolType == PoolType.Duration)
            StartCoroutine(CheckTime());
    }

    private void OnDisable()
    {
        if (_poolType == PoolType.Disabled)
            _poolInfo.Pool.Despawn(gameObject);
    }

    private IEnumerator CheckTime()
    {
        float elapsedTime = 0F;
        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _poolInfo.Pool.Despawn(gameObject);
    }
}