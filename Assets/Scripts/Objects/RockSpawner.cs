using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Objects
{
    public class RockSpawner : MonoBehaviour
    {
        private static RockSpawner _instance;

        public static RockSpawner Instance
        {
            get
            {
                if (_instance) return _instance;

                var go = new GameObject("RockSpawner");
                _instance = go.AddComponent<RockSpawner>();
                DontDestroyOnLoad(go);

                return _instance;
            }
        }

        private readonly Dictionary<Rock, IObjectPool<Rock>> _rockPools = new();
        private Transform _poolContainer;
        private readonly List<Rock> _activeRocks = new();

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _poolContainer = new GameObject("RockPoolContainer").transform;
            _poolContainer.parent = transform;
        }

        public IObjectPool<Rock> GetPoolForPrefab(Rock prefab)
        {
            if (_rockPools.TryGetValue(prefab, out var forPrefab))
                return forPrefab;

            var pool = new ObjectPool<Rock>(
                createFunc: () => CreateRock(prefab),
                actionOnGet: GetRockFromPool,
                actionOnRelease: ReleaseRock,
                actionOnDestroy: DestroyRock,
                defaultCapacity: 20,
                maxSize: 1000
            );

            _rockPools.Add(prefab, pool);
            return pool;
        }

        private Rock CreateRock(Rock prefab)
        {
            var rock = Instantiate(prefab, _poolContainer);
            rock.gameObject.SetActive(false);
            rock.SetPool(GetPoolForPrefab(prefab));
            return rock;
        }

        private void GetRockFromPool(Rock rock)
        {
            rock.gameObject.SetActive(true);
            _activeRocks.Add(rock);
        }

        private void ReleaseRock(Rock rock)
        {
            rock.transform.parent = _poolContainer;
            rock.gameObject.SetActive(false);
            _activeRocks.Remove(rock);
        }

        private void DestroyRock(Rock rock)
        {
            Destroy(rock);
        }

        public void ClearAllPools()
        {
            foreach (var pool in _rockPools.Values)
                pool.Clear();

            _rockPools.Clear();
        }
    }
}