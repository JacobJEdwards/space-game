#nullable enable

using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace NPC
{
    [RequireComponent(typeof(Health))]
    public class Life : MonoBehaviour, IPoolable<Life>
    {
        private IObjectPool<Life>? _lifePool;
        private Health _health = null!;

        private void Start()
        {
            _health = GetComponent<Health>();
            _health.onDeath.AddListener(OnDie);
        }

        public void SetPool(IObjectPool<Life> pool)
        {
            _lifePool = pool;
        }

        private void OnDie()
        {
            _health.Reset();
        }
    }
}