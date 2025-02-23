using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace NPC
{
    public class Life : MonoBehaviour, IPoolable<Life>
    {
        private IObjectPool<Life> _lifePool;
        private Health _health;

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
            gameObject.SetActive(false);
            _health.Reset();
        }
    }
}