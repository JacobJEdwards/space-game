#nullable enable

using System;
using UnityEngine;
using Movement.Config;
using Unity.Assertions;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class SpaceMovement : MonoBehaviour
    {
        [SerializeField] private SpaceMovementConfig config = null!;
        private InputManager _inputManager = null!;

        private Animator? _animator;

        private float _glide;
        private float _horizontalGlide;

        private Rigidbody _rb = null!;

        private float _rotationX;

        private float _verticalGlide;

        public float CurrentBoostAmount { get; private set; }


        private void Start()
        {
            _inputManager = InputManager.Instance;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            print(_animator);

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
        }

        private void HandleBoosting()
        {
            if (_inputManager.GetBoost() && CurrentBoostAmount > 0f)
            {
                CurrentBoostAmount -= config.BoostDepreciationRate;
            }
            else
            {
                if (CurrentBoostAmount < config.MaxBoostAmount) CurrentBoostAmount += config.BoostRechargeRate;
            }
        }

        private void HandleMovement()
        {
            var forward = Vector3.forward;
            var right = Vector3.right;
            var up = Vector3.up;

            if (forward == Vector3.zero && right == Vector3.zero && up == Vector3.zero)
            {
                _animator?.Play("Floating");
            }
            else
            {
                _animator?.Play("Idle");
            }

            // Roll
            _rb.AddRelativeTorque(Vector3.back * (_inputManager.GetRoll() * config.RollTorque * Time.fixedDeltaTime));

            // Pitch/Yaw
            _rb.AddRelativeTorque(Vector3.right * (Math.Clamp(-_inputManager.GetPitchYaw().y, -1f, 1f) *
                                                   config.PitchTorque * Time
                                                       .fixedDeltaTime));

            // Yaw
            _rb.AddRelativeTorque(Vector3.up * (Math.Clamp(_inputManager.GetPitchYaw().x, -1f, 1f) * config.YawTorque *
                                                Time
                                                    .fixedDeltaTime));


            // Thrust
            if (Mathf.Abs(_inputManager.GetForward()) > 0.1f)
            {
                var currentThrust = config.Thrust;
                if (_inputManager.GetBoost()) currentThrust *= config.BoostMultiplier;

                _rb.AddRelativeForce(forward * (_inputManager.GetForward() * currentThrust * Time.fixedDeltaTime));

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
                _rb.AddRelativeForce(up * (_inputManager.GetUpDown() * config.UpThrust * Time.fixedDeltaTime));
                _verticalGlide = _inputManager.GetUpDown() * config.UpThrust;
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
                                     (_inputManager.GetStrafe() * config.StrafeThrust * Time.fixedDeltaTime));

                _horizontalGlide = _inputManager.GetStrafe() * config.StrafeThrust;
            }
            else
            {
                _rb.AddRelativeForce(right * (_horizontalGlide * Time.fixedDeltaTime));
                _horizontalGlide *= config.LeftRightGlideReduction;
            }
        }
    }
}