using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private Image _dashCooldown;
    [SerializeField]
    private Image _dashAvailable;

    private void OnEnable()
    {
        _playerController.OnDashActiveChanged += OnDashActiveChanged;
    }

    private void OnDisable()
    {
        _playerController.OnDashActiveChanged -= OnDashActiveChanged;
    }

    private void OnDashActiveChanged(bool active)
    {
        _dashCooldown.gameObject.SetActive(active);
        _dashAvailable.gameObject.SetActive(!active);
    }

    private void Update()
    {
        if (_playerController.IsDashCooldown)
        {
            float dashRatio = _playerController.DashRemainingCoolTime / _playerController.DashCoolTime;
            _dashCooldown.fillAmount = dashRatio;
        }
    }
}
