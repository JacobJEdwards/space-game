using System;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using Interfaces;

namespace Objects
{
    public class Pool<T> : MonoBehaviour
    where T : MonoBehaviour, IPoolable<T>
    {
        public int maxCount;
        public T prefab;
        private ObjectPool<T> _pool;

        private readonly List<T> _activeObjs = new(100);

        private void Awake()
        {
            _pool = new ObjectPool<T>(
                CreateObject,
                OnGetFromPool,
                OnReturnToPool,
                OnDestroyObj,
                true,
                maxCount, maxCount
            );
        }

        private T CreateObject()
        {
            var obj = Instantiate(prefab);
            obj.SetPool(_pool);
            return obj;
        }

        private void OnGetFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            _activeObjs.Add(obj);
        }

        private void OnReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            _activeObjs.Add(obj);
        }

        private static void OnDestroyObj(T obj)
        {
            Destroy(obj.gameObject);
        }
    }
}