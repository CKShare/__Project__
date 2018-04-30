using UnityEngine;
using UnityEngine.UI;

public class AimUI : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;
    [SerializeField]
    private Image _aim;

    private void OnEnable()
    {
        _playerController.OnAimActiveChanged += OnAimActiveChanged;
    }

    private void OnDisable()
    {
        _playerController.OnAimActiveChanged -= OnAimActiveChanged;
    }

    private void OnAimActiveChanged(bool active)
    {
        _aim.gameObject.SetActive(active);
    }

}