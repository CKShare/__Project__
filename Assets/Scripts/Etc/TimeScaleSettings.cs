using UnityEngine;

[CreateAssetMenu]
public class TimeScaleSettings : ScriptableObject
{
    [SerializeField]
    private float _timeScale = 1F;
    [SerializeField]
    private float _duration = 0F;

    public float TimeScale => _timeScale;
    public float Duration => _duration;
}