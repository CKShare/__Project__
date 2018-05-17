using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class AimUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField, Required]
    private GameObject _aimUI;

    private void OnEnable()
    {
        _controller.OnAimActiveChanged += OnAimActiveChanged;
    }

    private void OnDisable()
    {
        _controller.OnAimActiveChanged -= OnAimActiveChanged;
    }

    private void OnAimActiveChanged(bool active)
    {
        _aimUI.SetActive(active);
    }
}
