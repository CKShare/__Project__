using UnityEngine;
using Sirenix.OdinInspector;

public class CoverPoint : MonoBehaviour
{
#if UNITY_EDITOR
    [Title("Debug | DO NOT TOUCH")]
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _height;

    private void OnDrawGizmos()
    {
        DebugExtension.DrawCapsule(transform.position + Vector3.up * _height, transform.position, Color.red, _radius);
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * (_height * 0.5F);
        Gizmos.DrawLine(origin, origin + transform.forward);
    }
#endif
}