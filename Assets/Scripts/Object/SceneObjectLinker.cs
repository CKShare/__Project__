using UnityEngine;

public class SceneObjectLinker : MonoBehaviour
{
    [SerializeField]
    private SceneObject _sceneObject;

    public SceneObject SceneObject => _sceneObject;
}