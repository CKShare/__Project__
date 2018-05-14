using UnityEngine;

public class RootLinker : MonoBehaviour
{
    [SerializeField]
    private GameObject _root;

    public GameObject Root => _root;
}
