using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class DamageSignUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField]
    private PoolInfo _dmgSign;

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _controller.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        _controller.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(GameObject attacker, GameObject victim, int damage)
    {
        Transform aTr = attacker.transform;
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(aTr.position);
        if (screenPos.x <= 0 || screenPos.y <= 0 || screenPos.x >= Screen.width || screenPos.y >= Screen.height) // Off-Screen
        {
            Transform vTr = victim.transform;
            Vector3 dir = aTr.position - vTr.position;
            Vector3 forward = _mainCamera.transform.forward;
            forward.y = dir.y = 0F;
            float angle = Vector3.SignedAngle(forward, dir, Vector3.up);

            RectTransform tr = _dmgSign.Pool.Spawn().GetComponent<RectTransform>();
            tr.rotation = Quaternion.Euler(0F, 0F, 270F - angle);
        }
    }
}
