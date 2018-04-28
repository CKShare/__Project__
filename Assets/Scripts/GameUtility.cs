using System.Collections;
using UnityEngine;
using Cinemachine;

public class GameUtility : MonoSingleton<GameUtility>
{
    private CameraShake _cameraShakeRef;
    private Coroutine _timeScaleCrt = null;

    protected override void Awake()
    {
        base.Awake();

        _cameraShakeRef = FindObjectOfType<CameraShake>();
    }

    public static void SetTimeScale(float timeScale, float duration)
    {
        if (Instance._timeScaleCrt != null)
            Instance.StopCoroutine(Instance._timeScaleCrt);
        Instance._timeScaleCrt = Instance.StartCoroutine(TimeScaleCrt(timeScale, duration));
    }

    private static IEnumerator TimeScaleCrt(float timeScale, float duration)
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
        Instance._timeScaleCrt = null;
    }

    public static void ShakeCamera(NoiseSettings noiseSettings, float duration)
    {
        Instance._cameraShakeRef.Shake(noiseSettings, duration);
    }

    public static Transform FindTargetInView(Collider[] targetPool, Vector3 position, Vector3 direction, float maxDistance, float maxAngle, LayerMask mask)
    {
        Transform target = null;
        int count = 0;
        if ((count = Physics.OverlapSphereNonAlloc(position, maxDistance, targetPool, mask)) > 0)
        {
            float nearestAngle = maxAngle;
            for (int i = 0; i < count; i++)
            {
                Vector3 diff = targetPool[i].transform.position - position;
                diff.y = 0F;
                float angle = Mathf.Acos(Vector3.Dot(direction.normalized, diff.normalized)) * Mathf.Rad2Deg;

                if (Mathf.Abs(angle) < nearestAngle)
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(position + new Vector3(0F, 1F, 0F), diff, out hitInfo, maxDistance))
                    {
                        if (hitInfo.transform == targetPool[i].transform)
                        {
                            nearestAngle = angle;
                            target = targetPool[i].transform;
                        }
                    }
                }
            }
        }

        return target;
    }
}