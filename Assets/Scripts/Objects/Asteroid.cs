using System.Collections.Generic;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Objects
{
    [RequireComponent(typeof(Health), typeof(Rigidbody), typeof(DropOnDeath))]
    public class Asteroid : MonoBehaviour, IPoolable<Asteroid>
    {
        [SerializeField] private GameObject rock;
        public Health health;

        public Rigidbody rb;
        [CanBeNull] private IObjectPool<Asteroid> _asteroidPool;
        private Fracture _fracture;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);
        }

        private void Start()
        {
            health = GetComponent<Health>();
            health.onDeath.AddListener(OnDie);
            _fracture = GetComponentInChildren<Fracture>();
            rb = GetComponent<Rigidbody>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void OnCollisionEnter(Collision other)
        {
            health.TakeDamage(other.relativeVelocity.magnitude);
        }

        public void SetPool(IObjectPool<Asteroid> pool)
        {
            _asteroidPool = pool;
        }

        private void OnDie()
        {
            Explode();
        }

        private void Explode()
        {
            _fracture?.FractureObject();

            if (_asteroidPool != null) _asteroidPool.Release(this);
            else Destroy(gameObject);

            health.Reset();
        }
    }
}