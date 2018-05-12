using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class WeaponSlot<TWeapon> where TWeapon : Weapon
{
    [SerializeField, Required]
    private GameObject _holder;
    [SerializeField, Required]
    private GameObject _grip;
    [SerializeField, AssetsOnly, Required]
    private TWeapon _weapon;

    public void Initialize(GameObject owner)
    {
        _holder.SetActive(false);
        _grip.SetActive(false);

        if (_weapon != null)
        {
            Weapon = GameObject.Instantiate(_weapon.gameObject).GetComponent<TWeapon>();
            Weapon.Owner = owner;
            SetActive(false);
        }
    }

    public void SetActive(bool active)
    {
        _holder.SetActive(!active);
        _grip.SetActive(active);
        Weapon.transform.SetParent(active ? _grip.transform : _holder.transform, false);
    }

    public TWeapon Weapon
    {
        get { return _weapon; }
        set { _weapon = value; }
    }
}