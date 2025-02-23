using System;
using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Objects
{
    [RequireComponent(typeof(Health), typeof(DropOnDeath))]
    public class Rock : MonoBehaviour, IPoolable<Rock>
    {
        private IObjectPool<Rock> _rockPool;
        private Health _health;

        private void Start()
        {
            _health = GetComponent<Health>();
            _health.onDeath.AddListener(OnDie);
        }


        public void SetPool(IObjectPool<Rock> pool)
        {
            _rockPool = pool;
        }

        private void OnDie()
        {
            gameObject.SetActive(false);
            _health.Reset();
        }

    }
}