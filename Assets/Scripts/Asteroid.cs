using UnityEngine;
using UnityEngine.Pool;

public class Asteroid : MonoBehaviour, Interfaces.IPoolable<Asteroid>
{
    public Health health;
    private ObjectPool<Asteroid> _asteroidPool;
    private Fracture _fracture;

    [SerializeField]
    private GameObject rock;

    public void SetPool(ObjectPool<Asteroid> pool)
    {
        _asteroidPool = pool;
    }

    private void Start()
    {
        health = GetComponentInChildren<Health>();
        health.onDeath.AddListener(OnDie);
        _fracture = GetComponentInChildren<Fracture>();
    }

    private void OnDie()
    {
        Explode();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision other)
    {
        health.TakeDamage(other.relativeVelocity.magnitude);
    }

    private void Explode()
    {
        _fracture?.FractureObject();
        _asteroidPool.Release(this);
        health.Reset();
    }
}