using UnityEngine;
using UnityEngine.UI;

//public class HealthBarUI : MonoBehaviour
//{
//    [SerializeField]
//    private PlayerController _playerController;
//    [SerializeField]
//    private Image _healthBar;

//    private void OnEnable()
//    {
//        _playerController.OnHealthChanged += OnHealthChanged;
//    }

//    private void OnDisable()
//    {
//        _playerController.OnHealthChanged -= OnHealthChanged;
//    }

//    private void OnHealthChanged(int newHealth)
//    {
//        float ratio = (float)newHealth / _playerController.MaxHealth;
//        _healthBar.fillAmount = ratio;
//        _healthBar.color = new Color(1F, ratio, ratio);
//    }
//}
