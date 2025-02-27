#nullable enable

using System;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace NPC
{
    [RequireComponent(typeof(Health), typeof(Rigidbody))]
    public class Life : MonoBehaviour, IPoolable<Life>
    {
        private IObjectPool<Life>? _lifePool;
        private Health _health = null!;
        private Rigidbody _rb = null!;

        public Transform? planet;
        public bool isGrounded;

        private Vector3 _surfaceNormal;

        [Header("Gravity")]
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float uprightSpeed = 2f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _health.onDeath.AddListener(OnDie);
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            UpdateGroundedState();
        }

        public void SetPool(IObjectPool<Life> pool)
        {
            _lifePool = pool;
        }

        private void OnDie()
        {
            _health.Reset();
        }

        private void UpdateGroundedState()
        {
            if (!planet) return;

            var direction = -(transform.position - planet.position).normalized;

            var origin = transform.position + _surfaceNormal * 0.1f;
            if (Physics.Raycast(origin, direction, out var hit, 5f))
            {
                _surfaceNormal = hit.normal;
                if (isGrounded) return;
                isGrounded = true;
            } else
            {
                isGrounded = false;
            }
        }

        private void ApplyGravity()
        {
            if (!planet) return;

            var gravityDir = -(transform.position - planet.position).normalized;
            _rb.AddForce(gravityDir * (Physics.gravity.magnitude * gravityMultiplier), ForceMode.Acceleration);

            var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * uprightSpeed);
        }

    }
}