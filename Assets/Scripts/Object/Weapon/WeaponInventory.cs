using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WeaponInventory
{
    [Serializable]
    private struct WeaponSlot
    {
        [SerializeField, Required]
        private GameObject _holder;
        [SerializeField, Required]
        private GameObject _grip;
        [SerializeField, AssetsOnly]
        private Weapon _weapon;

        public void Initialize(GameObject owner)
        {
            _holder.SetActive(false);
            _grip.SetActive(false);

            if (_weapon != null)
            {
                Weapon = GameObject.Instantiate(_weapon.gameObject).GetComponent<Weapon>();
                Weapon.Owner = owner;
                SetActive(true);
            }
        }

        public void SetActive(bool active)
        {
            _holder.SetActive(!active);
            _grip.SetActive(active);
            Weapon.transform.SetParent(active ? _grip.transform : _holder.transform, false);
        }

        public Weapon Weapon
        {
            get { return _weapon; }
            set { _weapon = value; }
        }
    }

    [SerializeField]
    private WeaponSlotType _initialWeaponType = WeaponSlotType.Primary;
    [SerializeField, DisableContextMenu]
    private Dictionary<WeaponSlotType, WeaponSlot> _slotDict = new Dictionary<WeaponSlotType, WeaponSlot>(new WeaponSlotTypeComparer());

    private Weapon _currentWeapon;
    private WeaponSlotType _currentWeaponType;

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

    public void SwapTo(WeaponSlotType slotType)
    {
        if (_currentWeaponType == slotType)
            return;

        WeaponSlot newSlot;
        if (!_slotDict.TryGetValue(slotType, out newSlot) || newSlot.Weapon == null)
            return;

        // Holster
        _slotDict[_currentWeaponType].SetActive(false);

        // Unholster
        newSlot.SetActive(true);
        _currentWeapon = newSlot.Weapon;
        _currentWeaponType = slotType;
    }

    public Weapon CurrentWeapon => _currentWeapon;
}