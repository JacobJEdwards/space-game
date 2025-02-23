using System;
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
        private readonly List<GameObject> _activeRocks = new ();
        private readonly Dictionary<GameObject, IObjectPool<GameObject>> _rockPools = new ();
        private readonly Dictionary<GameObject, Vector3> _rockPositions = new ();
        private Transform _playerTransform;

        [SerializeField] private float spawnRadius = 1000f;
        [SerializeField] private float despawnRadius = 1200f;
        [SerializeField] private float checkInterval = 1f;

        private float _nextCheckTime;

        private void Awake()
        {
            _planet = GetComponent<Planet>();
            _playerTransform = GameObject.FindWithTag("Player").transform;
        }

        private void Update()
        {
            if (Time.time < _nextCheckTime) return;

            UpdateRockVisibility();
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
                pos *= heightAtPoint;

                var rockPool = RockSpawner.Instance.GetPoolForPrefab(rockPrefab);
                var rock = rockPool.Get();
                rock.SetActive(false);
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

            if (playerDistance < spawnRadius)
            {
                foreach (var rockData in _rockPositions)
                {
                    var rock = rockData.Key;
                    var rockPos = rockData.Value;
                    var rockWorldPos = rockPos + planetWorldPos;

                    var distance = Vector3.Distance(_playerTransform.position, rockWorldPos);

                    if (distance < spawnRadius && !rock.activeSelf)
                    {
                        SpawnRock(rock, rockPos);
                    }
                    else if (distance > despawnRadius && rock.activeSelf)
                    {
                        DespawnRock(rock);
                    }
                }
            }
        }

        private void SpawnRock(GameObject rock, Vector3 rockPos)
        {
            rock.transform.parent = _planet.transform;
            rock.SetActive(true);
            rock.transform.localPosition = rockPos;
            rock.transform.localScale = Vector3.one * Random.Range(2f, 10f);
            rock.transform.rotation = Random.rotation;
            rock.SetActive(true);
            _activeRocks.Add(rock);
        }

        private void DespawnRock(GameObject rock)
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
    }
}