using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

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

        private Dictionary<GameObject, IObjectPool<GameObject>> _rockPools = new();
        private Transform _poolContainer;
        private List<GameObject> _activeRocks = new();

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

        public IObjectPool<GameObject> GetPoolForPrefab(GameObject prefab)
        {
            if (_rockPools.TryGetValue(prefab, out var forPrefab))
                return forPrefab;

            var pool = new ObjectPool<GameObject>(
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

        private GameObject CreateRock(GameObject prefab)
        {
            var rock = Instantiate(prefab, _poolContainer);
            rock.SetActive(false);
            return rock;
        }

        private void GetRockFromPool(GameObject rock)
        {
            rock.SetActive(true);
            _activeRocks.Add(rock);
        }

        private void ReleaseRock(GameObject rock)
        {
            rock.transform.parent = _poolContainer;
            rock.SetActive(false);
            _activeRocks.Remove(rock);
        }

        private void DestroyRock(GameObject rock)
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