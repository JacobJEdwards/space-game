#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Managers;
using Player;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;

namespace Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlanetaryMovement : MonoBehaviour, IUpgradeable
    {
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Idle = Animator.StringToHash("Idle");

        [SerializeField] private MovementSettings movementSettings = null!;
        [SerializeField] private CinemachineCamera playerCamera = null!;
        [SerializeField] private Transform head = null!;
        [SerializeField] private HeadBobbing headBobbing = null!;
        [SerializeField] private Jetpack jetpack = null!;

        [SerializeField] private Animator animator = null!;
        [SerializeField] private AnimationClip runAnimation = null!;
        [SerializeField] private AnimationClip walkAnimation = null!;
        [SerializeField] private AnimationClip jumpAnimation = null!;

        [SerializeField] private AudioSource movementClipSource = null!;
        [SerializeField] private AudioClip[] footstepClipsWalk = null!;
        [SerializeField] private AudioClip[] footstepClipsRun = null!;

        [SerializeField] private AudioClip jumpClip = null!;
        [SerializeField] private AudioClip jetpackClip = null!;
        [SerializeField] private AudioClip landClip = null!;

        public Transform? planetTransform;

        private readonly List<PlayerWalkingMovementUpgrade> _playerUpgrades = new();

        private float _currentRotationX;
        private InputManager _inputManager = null!;
        private bool _isGrounded;
        private bool _isSprinting;

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
            headBobbing.enabled = true;
        }

        private void OnDisable()
        {
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

        public bool CanApplyUpgrade(BaseUpgrade upgrade)
        {
            return upgrade is PlayerUpgrade or JetpackUpgrade;
        }

        public void ApplyUpgrade(BaseUpgrade upgrade)
        {
            switch (upgrade)
            {
                case JetpackUpgrade jetpackUpgrade:
                    ApplyJetpackUpgrade(jetpackUpgrade);
                    break;
                case PlayerWalkingMovementUpgrade playerUpgrade:
                    ApplyPlayerUpgrade(playerUpgrade);
                    break;
            }
        }

        public UpgradeType GetUpgradeType()
        {
            return UpgradeType.Player;
        }

        // TODO:
        // probably should cache
        private float JumpForce()
        {
            return _playerUpgrades.Aggregate(movementSettings.jumpForce,
                (current, upgrade) => current * upgrade.jumpHeightBonus);
        }

        private void ApplyPlayerUpgrade(PlayerWalkingMovementUpgrade playerUpgrade)
        {
            _playerUpgrades.Add(playerUpgrade);
        }

        public void ApplyJetpackUpgrade(JetpackUpgrade jetpackUpgrade)
        {
            jetpack.ApplyUpgrade(jetpackUpgrade);
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
                if (isGrounded && !_isGrounded) AudioManager.Instance.PlaySound(movementClipSource, landClip);
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
            if (jetpack.isJetpacking)
                HandleJetpack();
            else if (_isGrounded && planetTransform) HandleGroundMovement();
        }

        private void HandleGroundMovement()
        {
            var forward = _inputManager.GetForward();
            var strafe = _inputManager.GetStrafe();

            var moveDirection = transform.forward * forward + transform.right * strafe;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, _surfaceNormal).normalized;

            var slopeAngle = Vector3.Angle(_surfaceNormal, transform.up);
            if (!(slopeAngle <= movementSettings.maxSlopeAngle)) return;

            var currentSpeed = _isSprinting ? WalkSpeed() : RunSpeed();
            _rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);

            if (forward != 0 || strafe != 0)
            {
                if (Utils.IsNotPlaying(Walk, animator))
                    animator.Play(Walk);
                var clip = _isSprinting
                    ? Utils.RandomElement(footstepClipsRun)
                    : Utils.RandomElement(footstepClipsWalk);
                AudioManager.Instance.PlaySound(movementClipSource, clip);
            }
            else if (Utils.IsNotPlaying(Idle, animator))
            {
                animator.Play(Idle);
            }
        }

        private float WalkSpeed()
        {
            return _playerUpgrades.Aggregate(movementSettings.walkSpeed,
                (current, upgrade) => current * upgrade.speedBonus);
        }

        private float RunSpeed()
        {
            return _playerUpgrades.Aggregate(movementSettings.runSpeed,
                (current, upgrade) => current * upgrade.sprintSpeedBonus);
        }

        private void HandleJetpack()
        {
            jetpack.Handle(_inputManager.GetForward(), _inputManager.GetStrafe());
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

            _rb.AddForce(_surfaceNormal * JumpForce(), ForceMode.Impulse);
        }

        private void OnJumpPressed()
        {
            if (!_isGrounded) return;

            AudioManager.Instance.PlaySound(movementClipSource, jumpClip);
            Jump();
        }

        private void OnJetpackPressed()
        {
            if (!jetpack.CanJetpack()) return;

            jetpack.isJetpacking = true;
        }

        private void OnJetpackReleased()
        {
            jetpack.isJetpacking = false;
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
            public float groundCheckDistance = 0.1f;
            public float gravityMultiplier = 2.0f;
            public LayerMask groundLayer;
            public float maxSlopeAngle = 45.0f;
            public float mouseSensitivity = 2.0f;
            public float maxVerticalCameraAngle = 80.0f;
        }
    }
}