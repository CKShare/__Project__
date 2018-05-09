using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObject : MonoBehaviour
{
    [SerializeField, Required]
    private TextureType _textureType;

    public TextureType TextureType => _textureType;
}