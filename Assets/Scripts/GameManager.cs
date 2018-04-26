using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private int[] _sceneIndices = new int[0];
    [SerializeField]
    private CameraShake _cameraShakeRef;

    private Coroutine _timeScaleCrt = null;

    protected override void Awake()
    {
        if (!Application.isEditor)
        {
            foreach (var index in _sceneIndices)
                SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }
    }

    public void SetTimeScale(float timeScale, float duration)
    {
        if (_timeScaleCrt != null)
            StopCoroutine(_timeScaleCrt);
        _timeScaleCrt = StartCoroutine(TimeScaleCrt(timeScale, duration));
    }

    private IEnumerator TimeScaleCrt(float timeScale, float duration)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02F * timeScale;

        float elapsedTime = 0F;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1F;
        Time.fixedDeltaTime = 0.02F;
        _timeScaleCrt = null;
    }

    public void ShakeCamera(NoiseSettings noiseSettings, float duration)
    {
        _cameraShakeRef.Shake(noiseSettings, duration);
    }
}