using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SlowGunUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField, Required]
    private Image _fillImage;

    private void Update()
    {
        _fillImage.fillAmount = 1F - _controller.SlowGunRemainingTime / _controller.SlowGunCoolTime;
    }
}
