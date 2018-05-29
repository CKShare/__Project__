using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private int[] _sceneIndices = new int[0];

    private void Awake()
    {
        if (!Application.isEditor)
        {
            foreach (var index in _sceneIndices)
            {
                if (SceneManager.GetActiveScene().buildIndex == index)
                    continue;
                SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
            }
        }
    }
}