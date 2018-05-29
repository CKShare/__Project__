using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    private Transform _follow;
    [SerializeField]
    private float _heightOffset;

    private Transform _transform;
    private Transform _cameraTr;

    private void Awake()
    {
        _transform = transform;
        _cameraTr = Camera.main.transform;
    }

    private void Update()
    {
        Vector3 euler = _cameraTr.eulerAngles;
        _transform.position = _follow.position + Vector3.up * _heightOffset;
        _transform.rotation = Quaternion.Euler(0F, euler.y, 0F);
    }
}
