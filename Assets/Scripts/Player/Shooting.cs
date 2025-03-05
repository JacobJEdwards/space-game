#nullable enable

using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Movement;
using Player.Upgrades;
using Unity.Assertions;
using UnityEngine;
using Weapons;

namespace Player
{
    public class Shooting : MonoBehaviour, IUpgradeable
    {
        [SerializeField] private float laserMaxCharge = 10f;
        [SerializeField] private float laserHeatRate = 1f;
        [SerializeField] private float laserCoolRate = 2f;

        private readonly List<WeaponUpgrade> _upgrades = new();

        private bool _firing;

        private InputManager _inputManager = null!;
        private LaserFire[] _lasers = null!;

        private bool _overheated;

        private bool _targetInRange;

        public float LaserMaxCharge => laserMaxCharge;
        public float LaserCharge { get; private set; }

        private void Start()
        {
            LaserCharge = laserMaxCharge;
            _lasers = GetComponentsInChildren<LaserFire>(true);
            _inputManager = InputManager.Instance;

            _inputManager.SetOnShootPressed(OnFire);
            _inputManager.SetOnShootRelease(OnFireRelease);

            Assert.IsNotNull(_inputManager, "InputManager is not set!");
            Assert.IsNotNull(_lasers, "Lasers are not set!");
        }

        private void Update()
        {
            HandleLaserFiring();
            CoolLasers();
        }

        public bool CanApplyUpgrade(BaseUpgrade upgrade)
        {
            return upgrade is WeaponUpgrade;
        }

        public void ApplyUpgrade(BaseUpgrade upgrade)
        {
            if (upgrade is WeaponUpgrade weaponUpgrade) ApplyWeaponUpgrade(weaponUpgrade);
        }

        public UpgradeType GetUpgradeType()
        {
            return UpgradeType.Weapon;
        }

        private float MaxCharge()
        {
            return _upgrades.Aggregate(laserMaxCharge, (current, upgrade) => current * upgrade.maxChargeBonus);
        }

        private float HeatRate()
        {
            return _upgrades.Aggregate(laserHeatRate, (current, upgrade) => current * upgrade.heatRateBonus);
        }

        private float CoolRate()
        {
            return _upgrades.Aggregate(laserCoolRate, (current, upgrade) => current * upgrade.coolRateBonus);
        }

        // TODJ
        public float Range()
        {
            return _upgrades.Aggregate(0f, (current, upgrade) => current * upgrade.rangeBonus);
        }

        private void HandleLaserFiring()
        {
            if (_firing && !_overheated)
                FireLasers();
            else
                StopLasers();
        }

        private void CoolLasers()
        {
            if (_firing) return;

            var cooling = CoolRate() * Time.deltaTime;

            LaserCharge += cooling;

            if (LaserCharge >= MaxCharge() * 0.5f) _overheated = false;

            LaserCharge = Mathf.Clamp(LaserCharge, 0, MaxCharge());
        }

        private void FireLasers()
        {
            foreach (var laser in _lasers) laser.Fire();

            HeatLasers();
        }

        private void HeatLasers()
        {
            if (!_firing || _overheated) return;

            var heat = HeatRate() * Time.deltaTime;
            LaserCharge -= heat;

            if (LaserCharge > 0) return;

            _overheated = true;
            _firing = false;
        }

        private void StopLasers()
        {
            foreach (var laser in _lasers) laser.StopFire();
            _firing = false;
        }

        public void OnFire()
        {
            _firing = true;
        }

        public void OnFireRelease()
        {
            _firing = false;
        }

        private void ApplyWeaponUpgrade(WeaponUpgrade weaponUpgrade)
        {
            _upgrades.Add(weaponUpgrade);

            foreach (var laser in _lasers) laser.ApplyUpgrade(weaponUpgrade.damageBonus, weaponUpgrade.rangeBonus);
        }
    }
}