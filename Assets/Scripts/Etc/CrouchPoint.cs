using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CrouchPoint : MonoBehaviour
{
#if UNITY_EDITOR
    [Title("Debug | DO NOT TOUCH")]
    [SerializeField]
    private float _radius;
    [SerializeField]
    private float _height;
#endif

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();
        _canvas.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
            collider.SendMessage("OnCrouchPointEnter", transform);

        _canvas.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
            collider.SendMessage("OnCrouchPointExit", transform);

        _canvas.gameObject.SetActive(false);
    }

    private void OnPlayerCrouch()
    {
        _canvas.gameObject.SetActive(false);
    }

    private void OnPlayerStandUp()
    {
        _canvas.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        DebugExtension.DrawCapsule(transform.position + Vector3.up * _height, transform.position, Color.green, _radius);
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * (_height * 0.5F);
        Gizmos.DrawLine(origin, origin + transform.forward);
    }
}