using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObject : SerializedMonoBehaviour
{
    [SerializeField, Required, TitleGroup("Etc", order:1), Tooltip("오브젝트의 재질 타입")]
    private TextureType _textureType;

    public TextureType TextureType => _textureType;
}