using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class WeaponInventory
{
    [Serializable]
    private struct WeaponSlot
    {
        [SerializeField]
        private GameObject _holder;
        [SerializeField]
        private GameObject _grip;

        public void Initialize(GameObject owner)
        {
            // Destroy all the children of Grip.
            foreach (Transform child in _grip.transform)
                GameObject.Destroy(child.gameObject);

            // Take only one weapon from Holder.
            int childCount = _holder.transform.childCount;
            for (int i = 0; i < childCount - 1; i++)
                GameObject.Destroy(_holder.transform.GetChild(i).gameObject);

            // Set Owner.
            Weapon = _holder.transform.GetChild(childCount - 1).GetComponent<Weapon>();
            Weapon.Owner = owner;
        }

        public void SetActive(bool active)
        {
            _holder.SetActive(!active);
            _grip.SetActive(active);
            Weapon.transform.SetParent(active ? _grip.transform : _holder.transform, false);
        }

        public Weapon Weapon { get; set; }
    }

    [SerializeField]
    private WeaponType _initialWeaponType = WeaponType.Primary;
    [SerializeField, DisableContextMenu]
    private Dictionary<WeaponType, WeaponSlot> _slotDict = new Dictionary<WeaponType, WeaponSlot>();

    private Weapon _currentWeapon;
    private WeaponType _currentWeaponType;

    public void Initialize(GameObject owner)
    {
        foreach (var slotPair in _slotDict)
        {
            var slot = slotPair.Value;
            slot.Initialize(owner);
        }

        var initialSlot = _slotDict[_initialWeaponType];
        initialSlot.SetActive(true);
        _currentWeapon = initialSlot.Weapon;
        _currentWeaponType = _initialWeaponType;
    }

    public void SwapTo(WeaponType weaponType)
    {
        if (_currentWeaponType == weaponType || !_slotDict.ContainsKey(weaponType))
            return;

        // Holster
        _slotDict[_currentWeaponType].SetActive(false);

        // Unholster
        var newSlot = _slotDict[weaponType];
        newSlot.SetActive(true);
        _currentWeapon = newSlot.Weapon;
        _currentWeaponType = weaponType;
    }

    public Weapon CurrentWeapon => _currentWeapon;
}