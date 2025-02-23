using System;
using System.Collections.Generic;
using NPC;
using Objects;
using PlanetarySystem.Planet;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlanetLifeManager : MonoBehaviour
    {
        private Planet _planet;
        private readonly List<Life> _activeLife = new ();
        private readonly Dictionary<Life, IObjectPool<Life>> _lifePools = new ();
        private readonly Dictionary<Life, Vector3> _lifeSpawns = new ();
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
                UpdateLifeActive();
                _lastUpdatePosition = _playerTransform.position;
            }

            _nextCheckTime = Time.time + checkInterval;
        }

        public void GenerateLifeSpawns()
        {
            _lifeSpawns.Clear();
            if (_planet.lifePrefabs.Length == 0) return;

            for (var i = 0; i < Random.Range(5, 15); i++)
            {
                var lifePrefab = _planet.lifePrefabs[Random.Range(0, _planet.lifePrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = _planet.ShapeGenerator.GetScaledElevation(
                    _planet.ShapeGenerator.CalculateUnscaledElevation(pos)
                );
                pos *= heightAtPoint * 1.1f;

                var lifePool = LifeSpawner.Instance.GetPoolForPrefab(lifePrefab);
                var life = lifePool.Get();
                life.gameObject.SetActive(false);
                _lifeSpawns.Add(life, pos);
                _lifePools.Add(life, lifePool);
            }
        }

        private void UpdateLifeActive()
        {
            var planetWorldPos = _planet.transform.position;

            var playerDistance = Vector3.Distance(_playerTransform.position, planetWorldPos);

            if (playerDistance > despawnRadius)
            {
                DespawnAllLife();
                return;
            }

            if (!(playerDistance < spawnRadius)) return;

            foreach (var (life, lifePos) in _lifeSpawns)
            {
                var lifeWorldPos = lifePos + planetWorldPos;

                var distance = Vector3.Distance(_playerTransform.position, lifeWorldPos);

                if (distance < spawnRadius && !life.gameObject.activeSelf)
                {
                    SpawnLife(life, lifePos);
                }
                else if (distance > despawnRadius && life.gameObject.activeSelf)
                {
                    DespawnLife(life);
                }
            }
        }

        private void SpawnLife(Life life, Vector3 pos)
        {
            print("Spawning life");
            life.transform.parent = _planet.transform;
            life.gameObject.SetActive(true);
            life.transform.localPosition = pos;
            life.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
            life.gameObject.SetActive(true);

            var movement = life.GetComponent<NpcMovement>();
            movement.target = _playerTransform;
            movement.planet = _planet.transform;

            _activeLife.Add(life);
        }

        private void DespawnLife(Life life)
        {
            _lifePools[life].Release(life);
            _activeLife.Remove(life);
        }

        private void DespawnAllLife()
        {
            foreach (var life in _activeLife)
            {
                _lifePools[life].Release(life);
            }
            _activeLife.Clear();
        }

        public void ClearAllPools()
        {
            foreach (var pool in _lifePools.Values)
            {
                pool.Clear();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, despawnRadius);

            // draw positions
            foreach (var (_, pos) in _lifeSpawns)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}