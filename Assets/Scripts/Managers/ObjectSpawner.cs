#nullable enable

using Interfaces;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;


namespace Managers
{
    public class ObjectSpawner<T> : MonoBehaviour
    where T : MonoBehaviour, IPoolable<T>
    {
        private readonly Dictionary<T, IObjectPool<T>> _pools = new();
        private Transform? _poolContainer;

        private void Awake()
        {
            _poolContainer = new GameObject($"{typeof(T).Name}PoolContainer").transform;
            _poolContainer.parent = transform;
        }

        public IObjectPool<T> GetPoolForPrefab(T prefab)
        {
            if (_pools.TryGetValue(prefab, out var forPrefab))
                return forPrefab;

            var pool = new ObjectPool<T>(
                createFunc: () => CreateObject(prefab),
                actionOnGet: GetObjectFromPool,
                actionOnRelease: ReleaseObject,
                actionOnDestroy: DestroyObject,
                defaultCapacity: GetDefaultCapacity(),
                maxSize: GetMaxSize()
            );

            _pools.Add(prefab, pool);
            return pool;
        }

        protected virtual int GetDefaultCapacity() => 20;
        protected virtual int GetMaxSize() => 100;

        protected virtual T CreateObject(T prefab)
        {
            var obj = Instantiate(prefab, _poolContainer);
            obj.gameObject.SetActive(false);
            obj.SetPool(GetPoolForPrefab(prefab));
            return obj;
        }

        private void GetObjectFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void ReleaseObject(T obj)
        {
            obj.transform.parent = _poolContainer;
            obj.gameObject.SetActive(false);
        }

        protected virtual void DestroyObject(T obj)
        {
            Destroy(obj.gameObject);
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }
        }
    }
}