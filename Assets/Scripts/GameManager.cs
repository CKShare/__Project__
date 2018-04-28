using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private int[] _sceneIndices = new int[0];

    protected override void Awake()
    {
        base.Awake();

        if (!Application.isEditor)
        {
            foreach (var index in _sceneIndices)
                SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        }
    }
}