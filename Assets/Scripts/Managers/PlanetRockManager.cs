#nullable enable

using Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlanetRockManager : PlanetObjectManager<Rock>
    {
        public override void GenerateObjectPositions()
        {
            Positions.Clear();
            if (Planet.rockPrefabs.Length == 0) return;
            if (Planet.numRocks == 0) return;

            for (var i = 0; i < Planet.numRocks; i++)
            {
                var rockPrefab = Planet.rockPrefabs[Random.Range(0, Planet.rockPrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = Planet.ShapeGenerator.GetScaledElevation(
                    Planet.ShapeGenerator.CalculateUnscaledElevation(pos)
                );
                pos *= heightAtPoint * 1.1f;

                var rockPool = ObjectSpawner.GetPoolForPrefab(rockPrefab);
                var rock = rockPool.Get();
                rock.gameObject.SetActive(false);
                Positions.Add(rock, pos);
                Pools.Add(rock, rockPool);
            }
        }


        protected override void Spawn(Rock rock)
        {
            rock.transform.localScale = Vector3.one * Random.Range(2f, 10f);
            rock.transform.rotation = Random.rotation;
        }
    }
}