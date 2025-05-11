#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Movement.Config;
using Player;
using Spaceship;
using Unity.Assertions;
using UnityEngine;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpaceMovement : MonoBehaviour
    {
        [SerializeField] private SpaceMovementConfig config = null!;
        [SerializeField] private Thrusters? thrusters;
        [SerializeField] private Hyperdrive? hyperdrive;

        private readonly List<ISpaceMovementUpgrade> _upgrades = new();

        private Animator? _animator;

        private float _glide;
        private float _horizontalGlide;
        private InputManager _inputManager = null!;

        private Rigidbody _rb = null!;

        private float _rotationX;

        private float _verticalGlide;

        public float CurrentBoostAmount { get; private set; }


        private void Start()
        {
            _inputManager = InputManager.Instance;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            thrusters ??= GetComponentInChildren<Thrusters>();
            hyperdrive ??= GetComponentInChildren<Hyperdrive>();

            _inputManager.SetOnHyperdrivePressed(OnHyperdrivePressed);

            CurrentBoostAmount = config.MaxBoostAmount;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Assert.IsNotNull(config, "Config is not assigned");
            Assert.IsNotNull(_inputManager, "InputManager is not assigned");
            Assert.IsNotNull(_rb, "RB is not assigned");
        }

        private void FixedUpdate()
        {
            HandleBoosting();
            HandleMovement();
            // HandleFOV();
        }

        private void OnHyperdrivePressed()
        {
            if (!hyperdrive) return;

            if (hyperdrive.IsRepaired())
                hyperdrive.ActivateHyperdrive();
            else
                UiManager.Instance.SetWarning("Hyperdrive is damaged!", 2f);
        }

        public void ApplyUpgrade(ISpaceMovementUpgrade upgrade)
        {
            _upgrades.Add(upgrade);
        }

        private void HandleFOV()
        {
            var controller = CameraController.Instance;
            var cam = controller.ActiveCamera;

            if (!cam) return;

            var fov = cam.Lens.FieldOfView;

            fov = Mathf.Lerp(fov, _inputManager.GetBoost() ? 120f : 60f, Time.fixedDeltaTime);

            cam.Lens.FieldOfView = fov;
        }

        private float BoostDepreciationRate()
        {
            return config.BoostDepreciationRate *
                   _upgrades.Aggregate(1f, (cur, next) => cur * next.BoostDeprecationRateBonus);
        }

        private float MaxBoostAmount()
        {
            return config.MaxBoostAmount *
                   _upgrades.Aggregate(1f, (cur, next) => cur * next.MaxChargeBonus);
        }

        private float BoostRechargeRate()
        {
            return config.BoostRechargeRate *
                   _upgrades.Aggregate(1f, (cur, next) => cur * next.BoostRechargeRateBonus);
        }

        private void HandleBoosting()
        {
            if (_inputManager.GetBoost() && CurrentBoostAmount > 0f)
            {
                CurrentBoostAmount -= BoostDepreciationRate();
            }
            else
            {
                if (CurrentBoostAmount < MaxBoostAmount()) CurrentBoostAmount += BoostRechargeRate();
            }
        }

        private float Handling()
        {
            return _upgrades.Aggregate(1f, (cur, next) => cur * next.HandlingBonus);
        }

        private float Speed()
        {
            return _upgrades.Aggregate(1f, (cur, next) => cur * next.SpeedBonus);
        }

        private float Acceleration()
        {
            return _upgrades.Aggregate(1f, (cur, next) => cur * next.AccelerationBonus);
        }

        private float Boost()
        {
            return config.BoostMultiplier * _upgrades.Aggregate(1f, (cur, next) => cur * next.BoostBonus);
        }


        private void HandleMovement()
        {
            var forward = Vector3.forward;
            var right = Vector3.right;
            var up = Vector3.up;

            if (forward == Vector3.zero && right == Vector3.zero && up == Vector3.zero)
                _animator?.Play("Floating");
            else
                _animator?.Play("Idle");

            var roll = _inputManager.GetRoll();
            var pitchYaw = _inputManager.GetPitchYaw();
            var forwardInput = _inputManager.GetForward();
            var upDown = _inputManager.GetUpDown();
            var strafe = _inputManager.GetStrafe();

            if (thrusters && !thrusters.IsRepaired() && (roll != 0 || pitchYaw != Vector2.zero || forwardInput != 0 ||
                                                         upDown != 0 || strafe != 0))
            {
                UiManager.Instance.SetWarning("Thrusters are damaged!", 2f);
                return;
            }

            // Roll
            _rb.AddRelativeTorque(Vector3.back *
                                  (_inputManager.GetRoll() * config.RollTorque * Handling() * Time.fixedDeltaTime));

            // Pitch/Yaw
            _rb.AddRelativeTorque(Vector3.right * (Math.Clamp(-_inputManager.GetPitchYaw().y, -1f, 1f) *
                                                   config.PitchTorque * Handling() * Time
                                                       .fixedDeltaTime));

            // Yaw
            _rb.AddRelativeTorque(Vector3.up * (Math.Clamp(_inputManager.GetPitchYaw().x, -1f, 1f) * config.YawTorque
                * Handling() *
                Time
                    .fixedDeltaTime));


            // Thrust
            if (Mathf.Abs(_inputManager.GetForward()) > 0.1f)
            {
                var currentThrust = config.Thrust;
                if (_inputManager.GetBoost()) currentThrust *= Boost();

                _rb.AddRelativeForce(forward * (_inputManager.GetForward() * currentThrust * Acceleration() * Time
                    .fixedDeltaTime));

                _glide = currentThrust;
            }
            else
            {
                _rb.AddRelativeForce(forward * (_glide * Time.fixedDeltaTime));
                _glide *= config.ThrustGlideReduction;
            }

            // Up/Down
            if (Mathf.Abs(_inputManager.GetUpDown()) > 0.1f)
            {
                _rb.AddRelativeForce(up * (_inputManager.GetUpDown() * config.UpThrust * Acceleration() *
                                           Time.fixedDeltaTime));
                _verticalGlide = _inputManager.GetUpDown() * config.UpThrust * Acceleration();
            }
            else
            {
                _rb.AddRelativeForce(up * (_verticalGlide * Time.fixedDeltaTime));
                _verticalGlide *= config.UpDownGlideReduction;
            }

            // Strafe
            if (Mathf.Abs(_inputManager.GetStrafe()) > 0.1f)
            {
                _rb.AddRelativeForce(right *
                                     (_inputManager.GetStrafe() * config.StrafeThrust * Handling() *
                                      Time.fixedDeltaTime));

                _horizontalGlide = _inputManager.GetStrafe() * config.StrafeThrust * Handling();
            }
            else
            {
                _rb.AddRelativeForce(right * (_horizontalGlide * Time.fixedDeltaTime));
                _horizontalGlide *= config.LeftRightGlideReduction;
            }

            MaybeSlowdown();
        }

        private void MaybeSlowdown()
        {
            if (!Physics.Raycast(transform.position, transform.forward, config.SlowdownDistance, LayerMask.GetMask
                    ("PlanetSurface", "Water"))) return;

            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.one * 10f, Time.fixedDeltaTime);
        }
    }
}