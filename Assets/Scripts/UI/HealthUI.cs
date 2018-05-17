using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class HealthUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField, Required]
    private Text _text;
    [SerializeField, Required]
    private Image _fillImage;
    [SerializeField]
    private float _minFillAmount, _maxFillAmount;

    private void OnEnable()
    {
        _controller.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        _controller.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float newHealth)
    {
        float ratio = _minFillAmount + (newHealth / _controller.MaxHealth) * (_maxFillAmount - _minFillAmount);
        _fillImage.fillAmount = ratio;
        _text.text = ((int)newHealth).ToString(); // Make garbage.
    }
}