using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.15F);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}