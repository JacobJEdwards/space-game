#nullable enable

using System;
using Managers;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement
{
    // TODO: sometimes cant get up, guess surface normal gets messed up

    [RequireComponent(typeof(Rigidbody))]
    public class PlanetaryMovement : MonoBehaviour
    {
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Idle = Animator.StringToHash("Idle");

        [SerializeField] private MovementSettings movementSettings = null!;
        private InputManager _inputManager = null!;
        [SerializeField] private CinemachineCamera playerCamera = null!;
        [SerializeField] private Transform head = null!;
        [SerializeField] private HeadBobbing headBobbing = null!;
        [SerializeField] private Animator animator = null!;

        [SerializeField] private AudioSource movementClipSource = null!;

        [SerializeField] private AudioClip[] footstepClipsWalk = null!;
        [SerializeField] private AudioClip[] footstepClipsRun = null!;

        [SerializeField] private AudioClip jumpClip = null!;
        [SerializeField] private AudioClip jetpackClip = null!;
        [SerializeField] private AudioClip landClip = null!;

        private float _currentRotationX;
        private bool _isGrounded;
        private bool _isJetpacking;
        private bool _isSprinting;
        private float _jetpackFuel;

        public Transform? planetTransform;

        private Rigidbody _rb = null!;

        private Vector3 _surfaceNormal;

        private void Start()
        {
            _inputManager = InputManager.Instance;
            Assert.IsNotNull(movementSettings, "MovementSettings is missing");
            Assert.IsNotNull(_inputManager, "InputManager is missing");
            Assert.IsNotNull(playerCamera, "PlayerCamera is missing");
            Assert.IsNotNull(head, "Head is missing");
            Assert.IsNotNull(headBobbing, "HeadBobbing is missing");
            Assert.IsNotNull(animator, "Animator is missing");

            _rb = GetComponent<Rigidbody>();

            _jetpackFuel = movementSettings.jetpackFuel;

            CheckComponents();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _inputManager.SetOnJumpPressed(OnJumpPressed);

            _inputManager.SetOnJetpackPress(OnJetpackPressed);
            _inputManager.SetOnJetpackRelease(OnJetpackReleased);

            _inputManager.SetOnSprintPress(OnSprintPressed);
        }

        private void Update()
        {
            HandleCameraRotation();
        }

        private void FixedUpdate()
        {
            if (!planetTransform) FindPlanet();

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
            Assert.IsNotNull(_inputManager, "InputManager is missing");
            Assert.IsNotNull(playerCamera, "PlayerCamera is missing");
            Assert.IsNotNull(head, "Head is missing");
            Assert.IsNotNull(headBobbing, "HeadBobbing is missing");
        }

        private void HandleCameraRotation()
        {
            if (!head || !_inputManager) return;

            var pitchYaw = _inputManager.GetPitchYaw();

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

            planetTransform = results[0].transform;
        }

        private void UpdateGroundedState()
        {
            if (!planetTransform) return;

            var direction = (planetTransform.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, direction, out var hit,
                    20, movementSettings.groundLayer))
            {
                var isGrounded = hit.distance <= movementSettings.groundCheckDistance + 0.1f;
                if (isGrounded && !_isGrounded)
                {
                    AudioManager.Instance.PlaySound(movementClipSource, landClip);
                }
                _isGrounded = isGrounded;
                _surfaceNormal = hit.normal;
            }
            else
            {
                _isGrounded = false;
            }

            if (headBobbing)
                headBobbing.enabled = _isGrounded;
        }

        private void HandleMovement()
        {
            if (_isJetpacking)
            {
                HandleJetpack();
            }
            else if (_isGrounded && planetTransform)
            {
                HandleGroundMovement();
            }
        }

        private void HandleGroundMovement()
        {
            var forward = _inputManager.GetForward();
            var strafe = _inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _surfaceNormal).normalized;

            var slopeAngle = Vector3.Angle(_surfaceNormal, transform.up);
            if (slopeAngle <= movementSettings.maxSlopeAngle)
            {
                var currentSpeed = _isSprinting ? movementSettings.runSpeed : movementSettings.walkSpeed;
                _rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);

                if (forward != 0 || strafe != 0)
                {
                    if (Utils.IsNotPlaying(Walk, animator))
                        animator.CrossFade(Walk, 0.1f);
                    var clip = _isSprinting ? Utils.RandomElement(footstepClipsRun) : Utils.RandomElement(footstepClipsWalk);
                    AudioManager.Instance.PlaySound(movementClipSource, clip);
                }
                else if (Utils.IsNotPlaying(Idle, animator))
                    animator.CrossFade(Idle, 0.1f);
            }

            _jetpackFuel = Mathf.Min(_jetpackFuel + movementSettings.jetpackFuelConsumptionRate * Time.deltaTime,
                movementSettings.jetpackFuel);
        }

        private void HandleJetpack()
        {
            if (_jetpackFuel <= 0) _isJetpacking = false;

            _rb.AddForce(transform.up * movementSettings.jetpackForce, ForceMode.Acceleration);
            _jetpackFuel -= movementSettings.jetpackFuelConsumptionRate * Time.deltaTime;

            var forward = _inputManager.GetForward();
            var strafe = _inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;

            _rb.AddForce(moveDirection * movementSettings.walkSpeed, ForceMode.Acceleration);
            AudioManager.Instance.PlaySound(movementClipSource, jetpackClip);
        }

        private void ApplyGravity()
        {
            if (!planetTransform) return;

            var gravityDir = -(transform.position - planetTransform.position).normalized;
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
            if (!_isGrounded) return;

            AudioManager.Instance.PlaySound(movementClipSource, jumpClip);
            Jump();
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