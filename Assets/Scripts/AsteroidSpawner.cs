using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject player;
    public GameObject asteroidPrefab;

    public float spawnRadius = 100f;
    public float despawnRadius = 200f;
    public int maxAsteroids = 100;

    private readonly List<GameObject> _asteroidPool = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        for (var i = 0; i < maxAsteroids; i++)
        {
            var asteroid = Instantiate(asteroidPrefab);
            asteroid.SetActive(false);
            _asteroidPool.Add(asteroid);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (var asteroid in from asteroid in _asteroidPool where asteroid.activeSelf let distance = Vector3.Distance(player.transform.position, asteroid.transform.position) where distance > despawnRadius select asteroid)
        {
            asteroid.SetActive(false);
        }

        while (ActiveAsteroidCount() < maxAsteroids)
        {
            ActivateAsteroid();
        }
    }

    private void ActivateAsteroid()
    {
        foreach (var asteroid in _asteroidPool.Where(asteroid => !asteroid.activeSelf))
        {
            asteroid.transform.position = player.transform.position + Random.insideUnitSphere * spawnRadius;
            asteroid.transform.rotation = Random.rotation;
            asteroid.transform.localScale = Vector3.one * Random.Range(1f, 10f);

            var rb = asteroid.GetComponent<Rigidbody>();
            rb.linearVelocity = Random.insideUnitSphere * Random.Range(1f, 10f);
            rb.angularVelocity = Random.insideUnitSphere * Random.Range(1f, 10f);

            asteroid.SetActive(true);
            break;
        }
    }

    private int ActiveAsteroidCount()
    {
        return _asteroidPool.Count(asteroid => asteroid.activeSelf);
    }
}
