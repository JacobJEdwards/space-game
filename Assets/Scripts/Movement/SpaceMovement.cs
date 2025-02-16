using System;
using UnityEngine;

namespace Movement
{
    public class SpaceMovement : MonoBehaviour
    {
        [SerializeField] private SpaceMovementConfig config;
        [SerializeField] private InputManager inputManager;

        private Camera _camera;

        private float _glide;
        private float _horizontalGlide;

        private Rigidbody _rb;

        private float _rotationX;

        private float _verticalGlide;

        public float CurrentBoostAmount { get; private set; }

        private void Start()
        {
            _rb = GetComponentInParent<Rigidbody>();
            _camera = Camera.main;

            CurrentBoostAmount = config.MaxBoostAmount;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FixedUpdate()
        {
            HandleBoosting();
            HandleMovement();
        }

        private void HandleBoosting()
        {
            if (inputManager.GetBoost() && CurrentBoostAmount > 0f)
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

            // Roll
            _rb.AddRelativeTorque(Vector3.back * (inputManager.GetRoll() * config.RollTorque * Time.fixedDeltaTime));

            // Pitch/Yaw
            _rb.AddRelativeTorque(Vector3.right * (Math.Clamp(-inputManager.GetPitchYaw().y, -1f, 1f) * config.PitchTorque * Time
                .fixedDeltaTime));

            // Yaw
            _rb.AddRelativeTorque(Vector3.up * (Math.Clamp(inputManager.GetPitchYaw().x, -1f, 1f) * config.YawTorque * Time
                .fixedDeltaTime));


            // Thrust
            if (Mathf.Abs(inputManager.GetForward()) > 0.1f)
            {
                var currentThrust = config.Thrust;
                if (inputManager.GetBoost()) currentThrust *= config.BoostMultiplier;

                _rb.AddRelativeForce(forward * (inputManager.GetForward() * currentThrust * Time.fixedDeltaTime));

                _glide = currentThrust;
            }
            else
            {
                _rb.AddRelativeForce(forward * (_glide * Time.fixedDeltaTime));
                _glide *= config.ThrustGlideReduction;
            }

            // Up/Down
            if (Mathf.Abs(inputManager.GetUpDown()) > 0.1f)
            {
                _rb.AddRelativeForce(up * (inputManager.GetUpDown() * config.UpThrust * Time.fixedDeltaTime));
                _verticalGlide = inputManager.GetUpDown() * config.UpThrust;
            }
            else
            {
                _rb.AddRelativeForce(up * (_verticalGlide * Time.fixedDeltaTime));
                _verticalGlide *= config.UpDownGlideReduction;
            }

            // Strafe
            if (Mathf.Abs(inputManager.GetStrafe()) > 0.1f)
            {
                    _rb.AddRelativeForce(right *
                                         (inputManager.GetStrafe() * config.StrafeThrust * Time.fixedDeltaTime));

                _horizontalGlide = inputManager.GetStrafe() * config.StrafeThrust;
            }
            else
            {
                _rb.AddRelativeForce(right * (_horizontalGlide * Time.fixedDeltaTime));
                _horizontalGlide *= config.LeftRightGlideReduction;
            }
        }
    }
}