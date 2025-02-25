#nullable enable

using System.Collections.Generic;
using Interfaces;
using PlanetarySystem.Planet;
using UnityEngine;
using UnityEngine.Pool;

namespace Managers
{
    [RequireComponent(typeof(Planet))]
    public abstract class PlanetObjectManager<T> : MonoBehaviour
    where T : MonoBehaviour, IPoolable<T>
    {
        protected Planet? Planet;
        protected ObjectSpawner<T>? ObjectSpawner;

        private readonly List<T> _activeObjects = new ();
        protected readonly Dictionary<T, IObjectPool<T>> Pools = new ();
        protected readonly Dictionary<T, Vector3> Positions = new ();

        protected Transform? PlayerTransform;
        private Vector3 _lastUpdatePosition;

        [SerializeField] private float spawnRadius = 1000f;
        [SerializeField] private float despawnRadius = 1200f;
        [SerializeField] private float checkInterval = 1f;
        [SerializeField] private float distanceCheck = 50f;

        private float _nextCheckTime;

        private void Awake()
        {
            Planet = GetComponent<Planet>();
            PlayerTransform = GameObject.FindWithTag("Player").transform;
            _lastUpdatePosition = PlayerTransform.position;
            ObjectSpawner = FindFirstObjectByType<ObjectSpawner<T>>();
        }

        private void Update()
        {
            if (!PlayerTransform) return;

            if (Time.time < _nextCheckTime) return;

            var distanceMoved = Vector3.Distance(PlayerTransform.position, _lastUpdatePosition);

            if (distanceMoved >= distanceCheck)
            {
                UpdateVisibility();
                _lastUpdatePosition = PlayerTransform.position;
            }

            _nextCheckTime = Time.time + checkInterval;
        }

        private void OnDestroy()
        {
            DespawnAll();
        }

        public abstract void GenerateObjectPositions();

        private void UpdateVisibility()
        {
            if (!PlayerTransform || !Planet) return;

            var planetWorldPos = Planet.transform.position;
            var playerDistance = Vector3.Distance(PlayerTransform.position, planetWorldPos);

            if (playerDistance > despawnRadius)
            {
                DespawnAll();
                return;
            }

            if (!(playerDistance < spawnRadius)) return;

            foreach (var (obj, pos) in Positions)
            {
                var rockWorldPos = pos + planetWorldPos;
                var distance = Vector3.Distance(PlayerTransform.position, rockWorldPos);

                if (distance < spawnRadius && !obj.gameObject.activeSelf)
                {
                    SpawnObj(obj, pos);
                }
                else if (distance > despawnRadius && obj.gameObject.activeSelf)
                {
                    DespawnObj(obj);
                }
            }
        }

        private void DespawnAll()
        {
            foreach (var obj in _activeObjects)
            {
                Pools.TryGetValue(obj, out var pool);
                pool?.Release(obj);
            }

            Pools.Clear();
            _activeObjects.Clear();
        }

        private void SpawnObj(T obj, Vector3 pos)
        {
            if (!Planet) return;

            obj.transform.parent = Planet.transform;
            obj.gameObject.SetActive(true);
            obj.transform.localPosition = pos;
            _activeObjects.Add(obj);
            Spawn(obj);
        }

        protected abstract void Spawn(T obj);

        private void DespawnObj(T obj)
        {
            Pools[obj].Release(obj);
            _activeObjects.Remove(obj);
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, despawnRadius);

            foreach (var (_, pos) in Positions)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(pos, 10f);
            }
        }
    }
}