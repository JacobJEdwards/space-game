#nullable enable

using System.Collections.Generic;
using System.Linq;
using Movement;
using Player;
using UnityEngine;
using UnityEngine.Assertions;
using Weapons;

namespace Spaceship
{
    [RequireComponent(typeof(ShipController))]
    public class ShipShooting : MonoBehaviour
    {
        [SerializeField] private float laserMaxCharge = 10f;
        [SerializeField] private float laserHeatRate = 1f;
        [SerializeField] private float laserCoolRate = 2f;

        private readonly List<ShipLaserUpgrade> _upgrades = new();

        private bool _firing;
        private InputManager _inputManager = null!;

        private List<LaserFire> _lasers = new();

        private bool _overheated;
        private ShipController _shipController = null!;

        public float LaserMaxCharge => MaxCharge();
        public float LaserCharge { get; private set; }

        private void Start()
        {
            _shipController = GetComponent<ShipController>();
            LaserCharge = laserMaxCharge;
            _lasers = GetComponentsInChildren<LaserFire>(true).ToList();
            _inputManager = FindFirstObjectByType<InputManager>();

            _inputManager.SetOnShootPressed(OnFire);
            _inputManager.SetOnShootRelease(OnFireRelease);

            Assert.IsNotNull(_shipController, "Ship controller is not set!");
            Assert.IsNotNull(_inputManager, "Input manager is not set!");
            Assert.IsNotNull(_lasers, "Lasers are not set!");
        }

        private void Update()
        {
            if (!_shipController.IsOccupied)
                StopLasers();
            else
                HandleLaserFiring();

            CoolLasers();
        }

        private float CoolRate()
        {
            return _upgrades.Aggregate(laserCoolRate, (current, upgrade) => current * upgrade.coolRateBonus);
        }

        private float HeatRate()
        {
            return _upgrades.Aggregate(laserHeatRate, (current, upgrade) => current * upgrade.heatRateBonus);
        }

        private float MaxCharge()
        {
            return _upgrades.Aggregate(laserMaxCharge, (current, upgrade) => current * upgrade.maxChargeBonus);
        }

        public void ApplyUpgrade(ShipLaserUpgrade upgrade)
        {
            _upgrades.Add(upgrade);

            foreach (var laser in _lasers) laser.ApplyUpgrade(upgrade.damageBonus, upgrade.rangeBonus);
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

        private void OnFireRelease()
        {
            _firing = false;
        }
    }
}