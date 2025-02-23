using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace NPC
{
    public class LifeSpawner : MonoBehaviour
    {
        private static LifeSpawner _instance;

        public static LifeSpawner Instance
        {
            get
            {
                if (_instance) return _instance;

                var go = new GameObject("LifeSpawner");
                _instance = go.AddComponent<LifeSpawner>();
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        private readonly Dictionary<Life, IObjectPool<Life>> _lifePools = new();
        private Transform _poolContainer;
        private readonly List<Life> _activeLife = new();


        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _poolContainer = new GameObject("LifePoolContainer").transform;
            _poolContainer.parent = transform;
        }


        public IObjectPool<Life> GetPoolForPrefab(Life prefab)
        {
            if (_lifePools.TryGetValue(prefab, out var forPrefab))
                return forPrefab;

            var pool = new ObjectPool<Life>(
                createFunc: () => CreateLife(prefab),
                actionOnGet: GetLifeFromPool,
                actionOnRelease: ReleaseLife,
                actionOnDestroy: DestroyLife,
                defaultCapacity: 20,
                maxSize: 100
            );

            _lifePools.Add(prefab, pool);
            return pool;
        }

        private Life CreateLife(Life prefab)
        {
            var life = Instantiate(prefab, _poolContainer);
            life.gameObject.SetActive(false);
            life.SetPool(GetPoolForPrefab(prefab));
            return life;
        }

        private void GetLifeFromPool(Life life)
        {
            life.gameObject.SetActive(true);
            _activeLife.Add(life);
        }

        private void ReleaseLife(Life life)
        {
            life.transform.parent = _poolContainer;
            life.gameObject.SetActive(false);
            _activeLife.Remove(life);
        }

        private void DestroyLife(Life life)
        {
            Destroy(life.gameObject);
        }

        public void ClearAllPools()
        {
            foreach (var pool in _lifePools.Values)
            {
                pool.Clear();
            }
        }
    }
}