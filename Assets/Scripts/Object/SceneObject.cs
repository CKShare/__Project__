using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObject : SerializedMonoBehaviour
{
    [SerializeField, Required, TitleGroup("Etc", order:1)]
    private TextureType _textureType;

    public TextureType TextureType => _textureType;
}