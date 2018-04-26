using System.Collections;
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class CameraShake : CinemachineExtension
{
    private bool mInitialized = false;
    private NoiseSettings mNoiseSettings = null;
    private float mNoiseTime = 0F;
    private float mDuration = 0F;
    private float mDurationLeft = 0F;
    private Vector3 mNoiseOffsets = Vector3.zero;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            if (!mInitialized)
                Initialize();

            if (mDurationLeft > 0F)
            {
                float shakeFactor = mDurationLeft / mDuration;
                
                mNoiseTime += deltaTime;
                state.PositionCorrection += state.CorrectedOrientation * GetCombinedFilterResults(
                        mNoiseSettings.PositionNoise, mNoiseTime, mNoiseOffsets) * shakeFactor;
                Quaternion rotNoise = Quaternion.Euler(GetCombinedFilterResults(
                            mNoiseSettings.OrientationNoise, mNoiseTime, mNoiseOffsets) * shakeFactor);
                state.OrientationCorrection = state.OrientationCorrection * rotNoise;

                //mDurationLeft = Mathf.Lerp(mDurationLeft, 0F, deltaTime * mTimeScale);
                mDurationLeft -= deltaTime;
            }
        }
    }

    private void Initialize()
    {
        mInitialized = true;
        mNoiseTime = 0;
        mNoiseOffsets = new Vector3(
                UnityEngine.Random.Range(-10000F, 10000F),
                UnityEngine.Random.Range(-10000F, 10000F),
                UnityEngine.Random.Range(-10000F, 10000F));
    }

    private static Vector3 GetCombinedFilterResults(
        NoiseSettings.TransformNoiseParams[] noiseParams, float time, Vector3 noiseOffsets)
    {
        float xPos = 0f;
        float yPos = 0f;
        float zPos = 0f;
        if (noiseParams != null)
        {
            for (int i = 0; i < noiseParams.Length; ++i)
            {
                NoiseSettings.TransformNoiseParams param = noiseParams[i];
                Vector3 timeVal = new Vector3(param.X.Frequency, param.Y.Frequency, param.Z.Frequency) * time;
                timeVal += noiseOffsets;

                Vector3 noise = new Vector3(
                        Mathf.PerlinNoise(timeVal.x, 0F) - 0.5F,
                        Mathf.PerlinNoise(timeVal.y, 0F) - 0.5F,
                        Mathf.PerlinNoise(timeVal.z, 0F) - 0.5F);

                xPos += noise.x * param.X.Amplitude;
                yPos += noise.y * param.Y.Amplitude;
                zPos += noise.z * param.Z.Amplitude;
            }
        }
        return new Vector3(xPos, yPos, zPos);
    }

    public void Shake(NoiseSettings noiseSettings, float duration)
    {
        mNoiseSettings = noiseSettings;
        mDuration = mDurationLeft = duration;
    }
}