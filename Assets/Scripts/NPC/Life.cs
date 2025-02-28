#nullable enable

using System;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

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
        private LayerMask _planetLayers;

        public Vector3 surfaceNormal;

        [Header("Gravity")]
        [SerializeField] private float gravityMultiplier = 1f;
        [SerializeField] private float uprightSpeed = 2f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _health.onDeath.AddListener(OnDie);
            _planetLayers = LayerMask.GetMask("PlanetSurface", "Water");
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
            ApplyGravity();
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

            var direction = (planet.position - transform.position).normalized;

            if (Physics.Raycast(transform.position + transform.up, direction, out var hit, 8f, _planetLayers))
            {
                surfaceNormal = hit.normal;
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

            var targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, surfaceNormal),
                surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, uprightSpeed * Time.fixedDeltaTime);
        }

    }
}