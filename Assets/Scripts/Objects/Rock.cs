#nullable enable
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.Pool;

namespace Objects
{
    [RequireComponent(typeof(Health), typeof(DropOnDeath))]
    public class Rock : MonoBehaviour, IPoolable<Rock>
    {
        [SerializeField] private AudioSource destroySound = null!;
        private Health _health = null!;
        private IObjectPool<Rock>? _rockPool;

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
            AudioManager.Instance.PlaySound(destroySound, destroySound.clip);
            gameObject.SetActive(false);
            _health.Reset();
        }
    }
}