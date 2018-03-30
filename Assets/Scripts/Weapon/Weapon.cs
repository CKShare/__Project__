using UnityEngine;
using Sirenix.OdinInspector;

public class Weapon : MonoBehaviour
{
    [SerializeField, Required, InlineEditor]
    private WeaponSettings _settings;
    [SerializeField]
    private LayerMask _sweepLayer;

    private Transform _transform;

    private bool _activeSweep = true;
    private Vector3 _prevPosition;

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        if (_activeSweep)
        {
            RaycastHit hitInfo;
            if (Sweep(out hitInfo))
            {
                Debug.Log("Sweep");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_settings != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.Scale(_settings.BoundSize, transform.localScale));
            Gizmos.DrawWireCube(_settings.BoundCenter, Vector3.one);
        }
    }

    private bool Sweep(out RaycastHit hitInfo)
    {
        Vector3 position = _transform.position + _transform.TransformVector(_settings.BoundCenter);
        Vector3 half = Vector3.Scale(_settings.BoundSize, _transform.localScale) * 0.5F;
        Vector3 diff = position - _prevPosition;
        
        _prevPosition = position;
        return Physics.BoxCast(position, half, diff, out hitInfo, _transform.rotation, diff.magnitude, _sweepLayer);
    }

    public void ActivateSweep(object eventParam)
    {


        _activeSweep = true;
        _prevPosition = _transform.position + _transform.TransformVector(_settings.BoundCenter);
    }

    public void DeactivateSweep()
    {
        _activeSweep = false;
    }
}

public interface IDamageable
{
    void TakeDamage();
}