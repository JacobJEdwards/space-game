using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Objects
{
    public class AsteroidSpawner : MonoBehaviour
    {
        public GameObject player;

        public List<Asteroid> asteroidPrefabs;
        public List<GameObject> possibleDrops;

        public float spawnRadius = 2000f;
        public float fromPlayerRadius = 200f;
        public float despawnRadius = 1000f;
        public int maxAsteroids = 200;
        private readonly List<Asteroid> _activeAsteroids = new(100);

        private IObjectPool<Asteroid> _asteroidPool;

        private void Awake()
        {
            _asteroidPool = new ObjectPool<Asteroid>(
                CreateAsteroid,
                OnGetFromPool,
                OnReturnToPool,
                OnDestroyAsteroid,
                true,
                maxAsteroids, maxAsteroids
            );
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            for (var i = 0; i < maxAsteroids; i++)
            {
                var asteroid = _asteroidPool.Get();
                _asteroidPool.Release(asteroid);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            for (var i = _activeAsteroids.Count - 1; i >= 0; i--)
            {
                var asteroid = _activeAsteroids[i];
                if (Vector3.Distance(asteroid.transform.position, player.transform.position) > despawnRadius)
                    _asteroidPool.Release(asteroid);
            }

            while (_activeAsteroids.Count < maxAsteroids) ActivateAsteroid();
        }

        private Asteroid CreateAsteroid()
        {
            var asteroid = Instantiate(asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)]);
            asteroid.SetPossibleDrops(possibleDrops);
            asteroid.SetPool(_asteroidPool);
            return asteroid;
        }

        private void OnGetFromPool(Asteroid asteroid)
        {
            asteroid.gameObject.SetActive(true);
            _activeAsteroids.Add(asteroid);
        }

        private void OnReturnToPool(Asteroid asteroid)
        {
            asteroid.gameObject.SetActive(false);
            _activeAsteroids.Remove(asteroid);
        }

        private static void OnDestroyAsteroid(Asteroid asteroid)
        {
            Destroy(asteroid.gameObject);
        }

        private void ActivateAsteroid()
        {
            var asteroid = _asteroidPool.Get();

            if (!asteroid) return;

            asteroid.transform.position = player.transform.position +
                                          Random.insideUnitSphere.normalized *
                                          Random.Range(spawnRadius, fromPlayerRadius);
            asteroid.transform.rotation = Random.rotation;
            asteroid.transform.localScale = Vector3.one * Random.Range(1f, 10f);

            var rb = asteroid.rb;
            rb.linearVelocity = Random.insideUnitSphere * Random.Range(1f, 5f);
            rb.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 5f);
        }
    }
}