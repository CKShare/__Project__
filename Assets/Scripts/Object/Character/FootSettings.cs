using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class FootSettings : SerializedScriptableObject
{
    [SerializeField]
    private LayerMask _rayMask;
    [SerializeField]
    private float _rayHeightOffset;
    [SerializeField, InlineEditor, Required]
    private EffectSettings _stepEffectSettings;

    public LayerMask RayMask => _rayMask;
    public float RayHeightOffset => _rayHeightOffset;
    public EffectSettings StepEffectSettings => _stepEffectSettings;
}