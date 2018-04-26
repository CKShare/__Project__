using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

//public class SkillUI : MonoBehaviour
//{
//    [SerializeField]
//    private PlayerController _playerController;
//    [SerializeField, TitleGroup("Slow Gun")]
//    private Image _slowGunCooldown;
//    [SerializeField, TitleGroup("Slow Gun")]
//    private Image _slowGunAvailable;
//    [SerializeField, TitleGroup("Dash")]
//    private Image _dashCooldown;
//    [SerializeField, TitleGroup("Dash")]
//    private Image _dashAvailable;

//    private void OnEnable()
//    {
//        _playerController.OnSlowGunActiveChanged += OnSlowGunActiveChanged;
//        _playerController.OnDashActiveChanged += OnDashActiveChanged;
//    }

//    private void OnDisable()
//    {
//        _playerController.OnSlowGunActiveChanged -= OnSlowGunActiveChanged;
//        _playerController.OnDashActiveChanged -= OnDashActiveChanged;
//    }

//    private void OnSlowGunActiveChanged(bool active)
//    {
//        _slowGunCooldown.gameObject.SetActive(active);
//        _slowGunCooldown.gameObject.SetActive(!active);
//    }

//    private void OnDashActiveChanged(bool active)
//    {
//        _dashCooldown.gameObject.SetActive(active);
//        _dashAvailable.gameObject.SetActive(!active);
//    }

//    private void Update()
//    {
//        if (_playerController.IsSlowGunCooldown)
//        {
//            float slowGunRatio = _playerController.SlowGunRemainingDelay / _playerController.SlowGunDelay;
//            _slowGunCooldown.fillAmount = slowGunRatio;
//        }

//        if (_playerController.IsDashCooldown)
//        {
//            float dashRatio = _playerController.DashRemainingDelay / _playerController.DashDelay;
//            _dashCooldown.fillAmount = dashRatio;
//        }
//    }
//}