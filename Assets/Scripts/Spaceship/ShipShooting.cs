#nullable enable

using System.Collections.Generic;
using System.Linq;
using Movement;
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

        private bool _firing;
        private InputManager _inputManager = null!;

        private List<LaserFire> _lasers = new ();

        private bool _overheated;
        private ShipController _shipController = null!;

        private bool _targetInRange;

        public float LaserMaxCharge => laserMaxCharge;
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

            var cooling = laserCoolRate * Time.deltaTime;

            LaserCharge += cooling;

            if (LaserCharge >= laserMaxCharge) _overheated = false;

            LaserCharge = Mathf.Clamp(LaserCharge, 0, laserMaxCharge);
        }

        private void FireLasers()
        {
            foreach (var laser in _lasers) laser.Fire();

            HeatLasers();
        }

        private void HeatLasers()
        {
            if (!_firing || _overheated) return;

            var heat = laserHeatRate * Time.deltaTime;
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