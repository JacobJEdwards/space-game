#nullable enable

using NPC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlanetLifeManager : PlanetObjectManager<Life>
    {
        public override void GenerateObjectPositions ()
        {
            Positions.Clear();
            if (Planet.lifePrefabs.Length == 0) return;
            if (!Planet.hasLife) return;

            for (var i = 0; i < Random.Range(5, 15); i++)
            {
                var lifePrefab = Planet.lifePrefabs[Random.Range(0, Planet.lifePrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = Planet.ShapeGenerator.GetScaledElevation(
                    Planet.ShapeGenerator.CalculateUnscaledElevation(pos)
                );
                pos *= heightAtPoint * 1.1f;

                var lifePool = ObjectSpawner.GetPoolForPrefab(lifePrefab);
                var life = lifePool.Get();
                life.gameObject.SetActive(false);
                Positions.Add(life, pos);
                Pools.Add(life, lifePool);
            }
        }

        protected override void Spawn(Life life)
        {
            life.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);

            var lifeComp = life.GetComponent<Life>();
            lifeComp.planet = Planet.transform;

            var movement = life.GetComponent<NpcMovement>();
            movement.player = PlayerTransform;
        }
    }
}