using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private int[] _sceneIndices = new int[0];

    protected override void Awake()
    {
        if (!Application.isEditor)
        {
            foreach (var index in _sceneIndices)
                SceneManager.LoadSceneAsync(index);
        }
    }
}