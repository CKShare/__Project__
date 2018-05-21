using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CrouchPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15F);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}