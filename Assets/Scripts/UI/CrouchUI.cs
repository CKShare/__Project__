using UnityEngine;
using Sirenix.OdinInspector;

public class CrouchUI : MonoBehaviour
{
    [SerializeField, Required]
    private PlayerController _controller;
    [SerializeField, Required]
    private GameObject _crouchUI;

    private void OnEnable()
    {
        _controller.OnCrouchActiveChanged += OnCrouchActiveChanged;
    }

    private void OnDisable()
    {
        _controller.OnCrouchActiveChanged -= OnCrouchActiveChanged;
    }

    private void OnCrouchActiveChanged(bool active)
    {
        _crouchUI.SetActive(active);
    }
}
