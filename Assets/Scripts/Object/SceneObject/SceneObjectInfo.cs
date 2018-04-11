using UnityEngine;

[CreateAssetMenu]
public class SceneObjectInfo : ScriptableObject
{
    [SerializeField]
    private string _textureName;
    [SerializeField]
    private EffectInfo _hitEffectInfo;

    public string TextureName => _textureName;
    public EffectInfo HitEffectInfo => _hitEffectInfo;
}