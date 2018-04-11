using System.Collections;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    private Pool<GameObject> _pool;

    protected override void Awake()
    {
        base.Awake();

        _pool = PoolManager.Instance["Audio"];
    }

    public void PlayAt(Vector3 position, AudioClip clip)
    {
        var instance = _pool.Spawn();
        instance.transform.position = position;

        var source = instance.GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();

        StartCoroutine(CheckPlaying(source));
    }

    private IEnumerator CheckPlaying(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return null;
        }

        _pool.Despawn(source.gameObject);
    }
}