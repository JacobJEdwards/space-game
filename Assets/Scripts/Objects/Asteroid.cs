using System.Collections.Generic;
using Interfaces;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Objects
{
    public class Asteroid : MonoBehaviour, IPoolable<Asteroid>
    {
        [SerializeField] private GameObject rock;
        public Health health;

        public Rigidbody rb;
        [CanBeNull] private IObjectPool<Asteroid> _asteroidPool;
        private Fracture _fracture;

        [SerializeField]
        private List<GameObject> possibleDrops;

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

        public void SetPossibleDrops(List<GameObject> drops)
        {
            possibleDrops = drops;
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

            var resourceIndex = Random.Range(0, possibleDrops.Count);
            var possibleDrop = possibleDrops[resourceIndex];
            for (var i = 0; i < Random.Range(1, 3); i++)
            {
                Instantiate(possibleDrop, transform.position + Random.insideUnitSphere, Random.rotation);
            }
        }
    }
}