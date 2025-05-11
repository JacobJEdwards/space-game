#nullable enable
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.Pool;

namespace Objects
{
    [RequireComponent(typeof(Health), typeof(DropOnDeath))]
    public class TreeObject : MonoBehaviour, IPoolable<TreeObject>
    {
        [SerializeField] private AudioSource destroySound = null!;
        private Health _health = null!;
        private IObjectPool<TreeObject>? _treePool;

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
            AudioManager.Instance.PlaySound(destroySound, destroySound.clip);
            gameObject.SetActive(false);
            _health.Reset();
        }
    }
}