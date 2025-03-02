using UnityEngine;
using UnityEngine.Pool;

namespace Weapons
{
    public class ParticlePool : MonoBehaviour
    {
        [SerializeField] public ParticleSystem particlePrefab;
        [SerializeField] public int poolSize = 20;

        private ObjectPool<ParticleSystem> pool;

        private void Start()
        {
            pool = new ObjectPool<ParticleSystem>(CreateFunc, OnGet, OnRelease, OnDestroyParticle, true, poolSize, poolSize);
        }

        private ParticleSystem CreateFunc()
        {
            var particle = Instantiate(particlePrefab);
            particle.gameObject.SetActive(false);
            return particle;
        }

        private void OnGet(ParticleSystem particle)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
        }

        private void OnRelease(ParticleSystem particle)
        {
            particle.gameObject.SetActive(false);
            particle.Stop();
        }

        private void OnDestroyParticle(ParticleSystem particle)
        {
            Destroy(particle.gameObject);
        }

        public ParticleSystem GetParticle()
        {
            return pool.Get();
        }

        public void ReleaseParticle(ParticleSystem particle)
        {
            pool.Release(particle);
        }
    }
}