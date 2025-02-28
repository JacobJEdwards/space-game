#nullable enable

using Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class PlanetTreeManager : PlanetObjectManager<Objects.TreeObject>
    {
        public override void GenerateObjectPositions()
        {
            Positions.Clear();
            if (Planet.rockPrefabs.Length == 0) return;
            if (Planet.numRocks == 0) return;

            for (var i = 0; i < Planet.numRocks; i++)
            {
                var treePrefab = Planet.treePrefabs[Random.Range(0, Planet.treePrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = Planet.ShapeGenerator.GetScaledElevation(
                    Planet.ShapeGenerator.CalculateUnscaledElevation(pos)
                );

                pos *= heightAtPoint;

                // calculate the rotation of the tree
                var rotation = Quaternion.FromToRotation(Vector3.up, pos);


                var treePool = ObjectSpawner.GetPoolForPrefab(treePrefab);
                var tree = treePool.Get();
                tree.gameObject.SetActive(false);
                Positions.Add(tree, pos);
                Rotations.Add(tree, rotation.eulerAngles);
                Pools.Add(tree, treePool);
            }
        }


        protected override void Spawn(Objects.TreeObject treeObject)
        {
            treeObject.transform.localScale = Vector3.one * Random.Range(2f, 10f);
        }
    }
}