using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class DashUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField, Required]
    private Image _fillImage;

    private void Update()
    {
        _fillImage.fillAmount = 1F - _controller.DashRemainingTime / _controller.DashCoolTime;
    }
}
