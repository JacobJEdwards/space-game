#nullable enable
using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Objects
{
    [RequireComponent(typeof(Health), typeof(DropOnDeath))]
    public class TreeObject : MonoBehaviour, IPoolable<TreeObject>
    {
        private IObjectPool<TreeObject>? _treePool;
        private Health _health = null!;

        private void Start()
        {
            _health = GetComponent<Health>();
            _health.onDeath.AddListener(OnDie);
        }

        public void SetPool(IObjectPool<TreeObject> pool)
        {
            _treePool = pool;
        }

        private void OnDie()
        {
            gameObject.SetActive(false);
            _health.Reset();
        }

    }
}