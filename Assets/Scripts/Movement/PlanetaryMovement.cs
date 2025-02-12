using System;
using UnityEngine;

namespace Movement
{
    public class PlanetaryMovement : MonoBehaviour
    {
        [System.Serializable]
        public class MovementSettings
        {
            public float walkSpeed = 2.0f;
            public float runSpeed = 6.0f;
            public float jumpForce = 8.0f;
            public float groundCheckDistance = 0.1f;
            public float gravityMultiplier = 2.0f;
            public LayerMask groundLayer;
            public float maxSlopeAngle = 45.0f;
        }

        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private SpaceInput spaceInput;

        private Rigidbody _rb;
        private bool _isGrounded;
        private Vector3 _surfaceNormal;
        private Transform _planetTransform;
        private Camera _camera;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _camera = Camera.main;
            spaceInput = GetComponent<SpaceInput>();
        }

        public void SetPlanet(Transform planet)
        {
            _planetTransform = planet;
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
            HandleMovement();
            ApplyGravity();
        }

        private void UpdateGroundedState()
        {
            if (Physics.Raycast(transform.position, -_camera.transform.up, out var hit,
                    20, movementSettings.groundLayer))
            {
                var distanceToGround = hit.distance;
                _surfaceNormal = hit.normal;
                _planetTransform = hit.transform;

                _isGrounded = distanceToGround <= movementSettings.groundCheckDistance;
            }
            else
            {
                print("Not grounded");
                _isGrounded = false;
            }
        }

        private void HandleMovement()
        {
            if (!_isGrounded) return;

            var cameraTransform = _camera.transform;
            var forward = Vector3.ProjectOnPlane(cameraTransform.forward, _surfaceNormal).normalized;
            var right = Vector3.ProjectOnPlane(cameraTransform.right, _surfaceNormal).normalized;

            var moveDir = (forward * spaceInput.forward + right * spaceInput.strafe).normalized;

            var angle = Vector3.Angle(transform.up, _surfaceNormal);
            if (angle > movementSettings.maxSlopeAngle) return;

            var speed = movementSettings.walkSpeed; // add more to space input or use different input
            _rb.AddForce(moveDir * speed, ForceMode.Acceleration);

            // if (spaceInput.jump && _isGrounded)
            // {
            //     _rb.AddForce(_surfaceNormal * movementSettings.jumpForce, ForceMode.Impulse);
            // }
        }

        private void ApplyGravity()
        {
            if (!_planetTransform) return;

            var gravityDir = _isGrounded ? -_surfaceNormal : (_planetTransform.position - transform.position).normalized;

            _rb.AddForce(gravityDir * (Physics.gravity.magnitude * movementSettings.gravityMultiplier), ForceMode.Acceleration);

            if (!_isGrounded) return;

            var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
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
    }
}