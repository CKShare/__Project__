using UnityEngine;
using Sirenix.OdinInspector;

public class SceneObject : SerializedMonoBehaviour
{
    [SerializeField, BoxGroup("Base")]
    private string _textureName;

    public string TextureName => _textureName;
}