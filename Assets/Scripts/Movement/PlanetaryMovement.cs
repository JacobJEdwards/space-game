using System;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement
{
    // TODO: sometimes cant get up, guess surface normal gets messed up

    [RequireComponent(typeof(Rigidbody))]
    public class PlanetaryMovement : MonoBehaviour
    {
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Idle = Animator.StringToHash("Idle");

        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private Transform head;
        [SerializeField] private HeadBobbing headBobbing;
        [SerializeField] private Animator animator;

        private float _currentRotationX;
        private bool _isGrounded;
        private bool _isJetpacking;
        private bool _isSprinting;
        private float _jetpackFuel;

        private Transform _planetTransform;

        private Rigidbody _rb;

        private Vector3 _surfaceNormal;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();

            _jetpackFuel = movementSettings.jetpackFuel;

            CheckComponents();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            inputManager.SetOnJumpPressed(OnJumpPressed);

            inputManager.SetOnJetpackPress(OnJetpackPressed);
            inputManager.SetOnJetpackRelease(OnJetpackReleased);

            inputManager.SetOnSprintPress(OnSprintPressed);
        }

        private void Update()
        {
            HandleCameraRotation();
        }

        private void FixedUpdate()
        {
            if (!_planetTransform) FindPlanet();

            UpdateGroundedState();
            HandleMovement();
            ApplyGravity();
        }

        private void OnEnable()
        {
            if (headBobbing)
                headBobbing.enabled = true;
        }

        private void OnDisable()
        {
            if (headBobbing)
                headBobbing.enabled = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, -transform.up * movementSettings.groundCheckDistance);

            if (!_isGrounded) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _surfaceNormal * 2f);
        }

        private void CheckComponents()
        {
            Assert.IsNotNull(inputManager, "InputManager is missing");
            Assert.IsNotNull(playerCamera, "PlayerCamera is missing");
            Assert.IsNotNull(head, "Head is missing");
            Assert.IsNotNull(headBobbing, "HeadBobbing is missing");
        }

        private void HandleCameraRotation()
        {
            var pitchYaw = inputManager.GetPitchYaw();

            _currentRotationX -= pitchYaw.y * movementSettings.mouseSensitivity;
            _currentRotationX = Mathf.Clamp(_currentRotationX, -movementSettings.maxVerticalCameraAngle,
                movementSettings.maxVerticalCameraAngle);

            transform.Rotate(Vector3.up * (pitchYaw.x * movementSettings.mouseSensitivity));

            head.localRotation = Quaternion.Euler(_currentRotationX, 0, 0);
        }

        private void FindPlanet()
        {
            var results = new Collider[1];
            var size = Physics.OverlapSphereNonAlloc(transform.position, 50, results, movementSettings.groundLayer);

            if (size <= 0) return;

            _planetTransform = results[0].transform;
        }

        private void UpdateGroundedState()
        {
            if (!_planetTransform) return;

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

            headBobbing.enabled = _isGrounded;
        }

        private void HandleMovement()
        {
            if (_isJetpacking)
            {
                HandleJetpack();
            }
            else if (_isGrounded && _planetTransform)
            {
                HandleGroundMovement();
            }
        }

        private void HandleGroundMovement()
        {
            var forward = inputManager.GetForward();
            var strafe = inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _surfaceNormal).normalized;

            var slopeAngle = Vector3.Angle(_surfaceNormal, transform.up);
            if (slopeAngle <= movementSettings.maxSlopeAngle)
            {
                var currentSpeed = _isSprinting ? movementSettings.runSpeed : movementSettings.walkSpeed;
                _rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);

                if (forward != 0 || strafe != 0)
                    animator.Play(Walk);
                else
                    animator.Play(Idle);
            }

            _jetpackFuel = Mathf.Min(_jetpackFuel + movementSettings.jetpackFuelConsumptionRate * Time.deltaTime,
                movementSettings.jetpackFuel);
        }

        private void HandleJetpack()
        {
            if (_jetpackFuel <= 0) _isJetpacking = false;

            _rb.AddForce(transform.up * movementSettings.jetpackForce, ForceMode.Acceleration);
            _jetpackFuel -= movementSettings.jetpackFuelConsumptionRate * Time.deltaTime;

            var forward = inputManager.GetForward();
            var strafe = inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;

            _rb.AddForce(moveDirection * movementSettings.walkSpeed, ForceMode.Acceleration);
        }

        private void ApplyGravity()
        {
            if (!_planetTransform) return;

            var gravityDir = -(transform.position - _planetTransform.position).normalized;
            _rb.AddForce(gravityDir * (Physics.gravity.magnitude * movementSettings.gravityMultiplier),
                ForceMode.Acceleration);

            if (!_isGrounded)
            {
                var targetUp = -gravityDir;
                var targetRot = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 2 * Time.fixedDeltaTime);
                return;
            }

            var targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, _surfaceNormal),
                _surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.fixedDeltaTime);
        }

        private void Jump()
        {
            Assert.IsTrue(_isGrounded, "Player is not grounded");
            _rb.AddForce(_surfaceNormal * movementSettings.jumpForce, ForceMode.Impulse);
        }

        private void OnJumpPressed()
        {
            if (_isGrounded) Jump();
        }

        private void OnJetpackPressed()
        {
            _isJetpacking = true;
        }

        private void OnJetpackReleased()
        {
            _isJetpacking = false;
        }

        private void OnSprintPressed()
        {
            _isSprinting = !_isSprinting;
            headBobbing.SetSprinting(_isSprinting);
        }

        [Serializable]
        public class MovementSettings
        {
            public float walkSpeed = 2.0f;
            public float runSpeed = 6.0f;
            public float jumpForce = 8.0f;
            public float jetpackForce = 2.0f;
            public float groundCheckDistance = 0.1f;
            public float gravityMultiplier = 2.0f;
            public LayerMask groundLayer;
            public float maxSlopeAngle = 45.0f;
            public float jetpackFuel = 100.0f;
            public float jetpackFuelConsumptionRate = 10.0f;
            public float mouseSensitivity = 2.0f;
            public float maxVerticalAngle = 89.0f;
            public float maxVerticalCameraAngle = 80.0f;
        }
    }
}