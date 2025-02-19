using System;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlanetaryMovement : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float walkSpeed = 2.0f;
            public float runSpeed = 6.0f;
            public float jumpForce = 8.0f;
            public float groundCheckDistance = 0.1f;
            public float gravityMultiplier = 2.0f;
            public LayerMask groundLayer;
            public float maxSlopeAngle = 45.0f;
            public float jetpackFuel = 100.0f;
            public float jetpackFuelConsumptionRate = 10.0f;
            public float mouseSensitivity = 2.0f;
            public float maxVerticalAngle = 89.0f;
        }

        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private CinemachineCamera playerCamera;

        private Rigidbody _rb;
        private bool _isGrounded;
        private bool _isSprinting;
        private bool _isJetpacking;
        private float _jetpackFuel;

        private Vector3 _surfaceNormal;
        private Transform _planetTransform;
        private float _currentRotationX;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _jetpackFuel = movementSettings.jetpackFuel;

            CheckComponents();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            inputManager.SetOnJumpPressed(OnJumpPressed);
            inputManager.SetOnBoostPressed(OnBoostPressed);
        }

        private void CheckComponents()
        {
            Assert.IsNotNull(inputManager, "InputManager is missing");
            Assert.IsNotNull(playerCamera, "PlayerCamera is missing");
        }

        private void Update()
        {
            HandleCameraRotation();
        }

        private void HandleCameraRotation()
        {
            var pitchYaw = inputManager.GetPitchYaw();

            // Handle vertical rotation (pitch) of the camera
            _currentRotationX -= pitchYaw.y * movementSettings.mouseSensitivity;
            _currentRotationX = Mathf.Clamp(_currentRotationX, -movementSettings.maxVerticalAngle, movementSettings.maxVerticalAngle);

            // Get the right vector for the camera based on our current orientation
            var right = Vector3.Cross(_surfaceNormal, transform.forward).normalized;

            // Calculate the pitch rotation around the right vector
            var pitchRotation = Quaternion.AngleAxis(_currentRotationX, right);

            // Apply the pitch to the camera while maintaining its position
            playerCamera.transform.rotation = transform.rotation * pitchRotation;

            // Handle horizontal rotation (yaw) of the player
            transform.Rotate(Vector3.up * (pitchYaw.x * movementSettings.mouseSensitivity));
        }

        private void FixedUpdate()
        {
            if (!_planetTransform)
            {
                FindPlanet();
            }

            UpdateGroundedState();
            print("Is Grounded: " + _isGrounded);
            HandleMovement();
            ApplyGravity();
        }

        private void FindPlanet()
        {
            var results = new Collider[1];
            var size = Physics.OverlapSphereNonAlloc(transform.position, 50, results, movementSettings.groundLayer);

            if (size > 0)
            {
                _planetTransform = results[0].transform;
            }
        }

        private void UpdateGroundedState()
        {
            var direction = (_planetTransform.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, direction, out var hit,
                    20, movementSettings.groundLayer))
            {
                _isGrounded = hit.distance <= movementSettings.groundCheckDistance + 0.1f;
                _surfaceNormal = hit.normal;
            }
            else
            {
                _isGrounded = false;
            }
        }


        private void UseJetpack()
        {
            if (_jetpackFuel <= 0)
            {
                _isJetpacking = false;
                return;
            }

            _isJetpacking = true;
            _jetpackFuel -= movementSettings.jetpackFuelConsumptionRate * Time.fixedDeltaTime;
            _rb.AddForce(_surfaceNormal * movementSettings.jumpForce, ForceMode.Impulse);
        }

        private void HandleMovement()
        {
            if (!_isGrounded && !_isJetpacking) return;

            var forward = inputManager.GetForward();
            var strafe = inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _surfaceNormal).normalized;

            var slopeAngle = Vector3.Angle(_surfaceNormal, transform.up);
            if (slopeAngle <= movementSettings.maxSlopeAngle)
            {
                var currentSpeed = _isSprinting ? movementSettings.runSpeed : movementSettings.walkSpeed;
                _rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);
            }

            if (_isGrounded)
            {
                _jetpackFuel = Mathf.Min(_jetpackFuel + movementSettings.jetpackFuelConsumptionRate * Time.deltaTime,
                    movementSettings.jetpackFuel);
            }
        }
        private void ApplyGravity()
        {
            var gravityDir = -(transform.position - _planetTransform.position).normalized;
            _rb.AddForce(gravityDir * (Physics.gravity.magnitude * movementSettings.gravityMultiplier), ForceMode.Acceleration);

            if (!_isGrounded) return;

            // rotate player to match the surface normal
            // TODO: fix !!
            var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.fixedDeltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * movementSettings.groundCheckDistance);

            // Draw surface normal when grounded
            if (!_isGrounded) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _surfaceNormal * 2f);
        }

        private void OnBoostPressed()
        {
            _isSprinting = !_isSprinting; // fix later
        }

        private void OnJumpPressed()
        {
            if (_isGrounded)
            {
                _rb.AddForce(_surfaceNormal * movementSettings.jumpForce, ForceMode.Impulse);
            }
            else
            {
                UseJetpack();
            }
        }
    }
}