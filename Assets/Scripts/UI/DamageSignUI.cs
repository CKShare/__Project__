using UnityEngine;
using UnityEngine.UI;

public class DamageSignUI : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private RectTransform _canvasRect;
    [SerializeField]
    private RectTransform _signRect;
    [SerializeField]
    private string _damageSign;

    private Camera _mainCamera;
    private Pool<GameObject> _damageSignPool;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _damageSignPool = PoolManager.Instance[_damageSign];
    }

    private void OnEnable()
    {
        _playerController.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        _playerController.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(Transform attacker, int damage)
    {
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(attacker.position);
        if (screenPos.x <= 0 || screenPos.y <= 0 || screenPos.x >= Screen.width || screenPos.y >= Screen.height) // Off-Screen
        {
            if (screenPos.z < 0F)
            {

            }

            Vector3 screenHalf = new Vector3(Screen.width, Screen.height, 0F) * 0.5F;
            Vector2 canvasHalf = new Vector2(_canvasRect.rect.width, _canvasRect.rect.height) * 0.5F;
            screenPos -= screenHalf;
            screenPos *= 0.5F;
            Debug.Log(_mainCamera.WorldToViewportPoint(attacker.position));
            screenPos.x = Mathf.Clamp(screenPos.x, -canvasHalf.x, canvasHalf.x);
            screenPos.y = Mathf.Clamp(screenPos.y, -canvasHalf.y, canvasHalf.y);

            RectTransform tr = _damageSignPool.Spawn().GetComponent<RectTransform>();
            tr.anchoredPosition = screenPos;
            tr.eulerAngles = new Vector3(0F, 0F, Mathf.Atan2(screenPos.y, screenPos.x) * Mathf.Rad2Deg + 180F);
        }
    }

}