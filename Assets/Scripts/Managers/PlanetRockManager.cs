using System.Collections.Generic;
using Objects;
using PlanetarySystem.Planet;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlanetRockManager : MonoBehaviour
    {
        private Planet _planet;
        private readonly List<Rock> _activeRocks = new ();
        private readonly Dictionary<Rock, IObjectPool<Rock>> _rockPools = new ();
        private readonly Dictionary<Rock, Vector3> _rockPositions = new ();
        private Transform _playerTransform;
        private Vector3 _lastUpdatePosition;

        [SerializeField] private float spawnRadius = 1000f;
        [SerializeField] private float despawnRadius = 1200f;
        [SerializeField] private float checkInterval = 1f;
        [SerializeField] private float distanceCheck = 50f;

        private float _nextCheckTime;

        private void Awake()
        {
            _planet = GetComponent<Planet>();
            _playerTransform = GameObject.FindWithTag("Player").transform;
            _lastUpdatePosition = _playerTransform.position;
        }

        private void Update()
        {
            if (Time.time < _nextCheckTime) return;

            var distanceMoved = Vector3.Distance(_playerTransform.position, _lastUpdatePosition);

            if (distanceMoved >= distanceCheck)
            {
                UpdateRockVisibility();
                _lastUpdatePosition = _playerTransform.position;
            }

            _nextCheckTime = Time.time + checkInterval;
        }

        public void GenerateRockPositions()
        {
            _rockPositions.Clear();
            if (_planet.rockPrefabs.Length == 0) return;
            if (_planet.numRocks == 0) return;

            for (var i = 0; i < _planet.numRocks; i++)
            {
                var rockPrefab = _planet.rockPrefabs[Random.Range(0, _planet.rockPrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = _planet.ShapeGenerator.GetScaledElevation(
                    _planet.ShapeGenerator.CalculateUnscaledElevation(pos)
                );
                pos *= heightAtPoint * 1.1f;

                var rockPool = RockSpawner.Instance.GetPoolForPrefab(rockPrefab);
                var rock = rockPool.Get();
                rock.gameObject.SetActive(false);
                _rockPositions.Add(rock, pos);
                _rockPools.Add(rock, rockPool);
            }
        }

        private void UpdateRockVisibility()
        {
            var planetWorldPos = _planet.transform.position;

            var playerDistance = Vector3.Distance(_playerTransform.position, planetWorldPos);

            if (playerDistance > despawnRadius)
            {
                DespawnAllRocks();
                return;
            }

            if (!(playerDistance < spawnRadius)) return;

            foreach (var (rock, rockPos) in _rockPositions)
            {
                var rockWorldPos = rockPos + planetWorldPos;
                var distance = Vector3.Distance(_playerTransform.position, rockWorldPos);

                if (distance < spawnRadius && !rock.gameObject.activeSelf)
                {
                    SpawnRock(rock, rockPos);
                }
                else if (distance > despawnRadius && rock.gameObject.activeSelf)
                {
                    DespawnRock(rock);
                }
            }
        }

        private void SpawnRock(Rock rock, Vector3 rockPos)
        {
            rock.transform.parent = _planet.transform;
            rock.gameObject.SetActive(true);
            rock.transform.localPosition = rockPos;
            rock.transform.localScale = Vector3.one * Random.Range(2f, 10f);
            rock.transform.rotation = Random.rotation;
            rock.gameObject.SetActive(true);
            _activeRocks.Add(rock);
        }

        private void DespawnRock(Rock rock)
        {
            _rockPools[rock].Release(rock);
            _activeRocks.Remove(rock);
        }

        private void DespawnAllRocks()
        {
            foreach (var rock in _activeRocks)
            {
                _rockPools[rock].Release(rock);
            }
            _activeRocks.Clear();
        }

        private void OnDestroy()
        {
            DespawnAllRocks();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, despawnRadius);

            foreach (var (_, rockPos) in _rockPositions)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(rockPos + transform.position, 10f);
            }
        }
    }
}