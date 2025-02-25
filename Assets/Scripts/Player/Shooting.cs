#nullable enable

using Movement;
using Unity.Assertions;
using UnityEngine;
using Weapons;

namespace Player
{
    public class Shooting : MonoBehaviour
    {
        [SerializeField] private float laserMaxCharge = 10f;
        [SerializeField] private float laserHeatRate = 1f;
        [SerializeField] private float laserCoolRate = 2f;

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

        public void OnFireRelease()
        {
            _firing = false;
        }
    }
}