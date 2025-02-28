#nullable enable

using System;
using System.Collections.Generic;
using Managers;
using Objects;
using Unity.Serialization;
using NPC;
using UnityEngine;

// TODO : SEED - RANDOM GEN
namespace PlanetarySystem.Planet
{
    public class Planet : MonoBehaviour
    {
        [Range(2, 256)] public int resolution = 10;
        [Range(0, 512)] public int numRocks = 40;
        [Range(0, 60)] public int numTrees = 40;

        public Material atmosphereMaterial = null!;

        public PlanetWater? waterSystem;
        public Material waterMaterial = null!;

        public bool hasWater;
        public bool hasLife;

        public Biome biome;

        [DontSerialize] public ShapeSettings shapeSettings = null!;
        [DontSerialize] public ColourSettings colourSettings = null!;

        [SerializeField] public Rock[] rockPrefabs = Array.Empty<Rock>();
        [SerializeField] public Life[] lifePrefabs = Array.Empty<Life>();
        [SerializeField] public Objects.TreeObject[] treePrefabs = Array.Empty<Objects.TreeObject>();

        [DontSerialize] private MeshFilter[] _meshFilters = Array.Empty<MeshFilter>();

        [DontSerialize] private TerrainFace[] _terrainFaces = Array.Empty<TerrainFace>();

        private GameObject _atmosphere = null!;
        private GameObject _atmosphereObject = null!;

        private readonly ColourGenerator _colourGenerator = new();
        public readonly ShapeGenerator ShapeGenerator = new();

        [SerializeField] private PlanetRockManager rockManager = null!;
        [SerializeField] private PlanetTreeManager treeManager = null!;
        [SerializeField] private PlanetLifeManager lifeManager = null!;

        private void Initialize()
        {
            ShapeGenerator.UpdateSettings(shapeSettings);
            _colourGenerator.UpdateSettings(colourSettings);

            if (_meshFilters.Length == 0)
            {
                _meshFilters = new MeshFilter[6];
            }

            _terrainFaces = new TerrainFace[6];

            Vector3[] directions =
                { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            for (var i = 0; i < 6; i++)
            {
                if (_meshFilters[i])
                {
                    Destroy(_meshFilters[i].gameObject);
                }

                var meshObj = new GameObject($"mesh_{i}")
                {
                    transform =
                    {
                        // parent = _meshContainer.transform,
                        parent = transform,
                        position = transform.position
                    }
                };

                meshObj.AddComponent<MeshRenderer>();
                _meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                var mesh = new Mesh
                {
                    name = $"PlanetMesh_{i}"
                };
                _meshFilters[i].sharedMesh = mesh;

                _meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

                _terrainFaces[i] =
                    new TerrainFace(ShapeGenerator, _meshFilters[i].sharedMesh, resolution, directions[i]);
                _meshFilters[i].gameObject.layer = (int)Layers.PlanetSurface;

            }

            gameObject.layer = (int)Layers.PlanetSurface;
        }

        public void GeneratePlanet()
        {
            if (biome == Biome.Ocean)
            {
                GenerateWater();
                GenerateAtmosphere();
            }
            else
            {

                Initialize();
                GenerateMesh();
                GenerateColours();
                GenerateAtmosphere();
                CombineMeshes();
                if (hasWater) GenerateWater();
                GenerateRocks();
                GenerateTrees();
                GenerateLife();

                // foreach (var meshFilter in _meshFilters)
                // {
                //     var col = meshFilter.gameObject.AddComponent<MeshCollider>();
                //     col.sharedMesh = meshFilter.sharedMesh;
                // }
            }
        }

        private void CombineMeshes()
        {
            var combines = new CombineInstance[_meshFilters.Length];

            for (var i = 0; i < _meshFilters.Length; i++)
            {
                combines[i].mesh = _meshFilters[i].sharedMesh;
                combines[i].transform = _meshFilters[i].transform.localToWorldMatrix;
                Destroy(_meshFilters[i]);
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combines, true, false);
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshFilter.sharedMesh.name = "PlanetMesh";

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = colourSettings.planetMaterial;

            var col = gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = meshFilter.sharedMesh;
        }


        // WATER IS CIRCULAR
        private void GenerateWater()
        {
            if (waterSystem) Destroy(waterSystem.gameObject);

            var water = new GameObject("PlanetWater")
            {
                transform =
                {
                    parent = transform,
                    position = transform.position
                }
            };

            waterSystem = water.AddComponent<PlanetWater>();
            waterSystem.GenerateWater(shapeSettings, waterMaterial);
        }

        private void GenerateAtmosphere()
        {
            if (_atmosphere)
                Destroy(_atmosphere);

            _atmosphere = new GameObject("Atmosphere")
            {
                transform =
                {
                    parent = transform,
                    position = transform.position,
                    localScale = Vector3.one,
                    localRotation = Quaternion.identity
                }
            };


            var sphere = _atmosphere.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = shapeSettings.planetRadius * 1.5f;

            _atmosphere.layer = (int)Layers.Atmosphere;

            if (_atmosphereObject)
            {
                Destroy(_atmosphereObject);
            }

            _atmosphereObject = new GameObject("AtmosphereMesh")
            {
                transform =
                {
                    parent = transform,
                    position = transform.position,
                    localScale = Vector3.one,
                    localRotation = Quaternion.identity
                }
            };

            var meshFilter = _atmosphereObject.AddComponent<MeshFilter>();
            var meshRenderer = _atmosphereObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = CreateSphereMesh(1f, 32, 24);

            var atmosphereScale = shapeSettings.planetRadius * 1.5f;
            _atmosphereObject.transform.localScale = new Vector3(atmosphereScale, atmosphereScale, atmosphereScale);

            meshRenderer.material = atmosphereMaterial;
        }

        private static Mesh CreateSphereMesh(float radius, int horizontalSegments, int verticalSegments)
        {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            for (var v = 0; v <= verticalSegments; v++)
            {
                var vt = v / (float)verticalSegments;
                var theta = vt * Mathf.PI;

                for (var h = 0; h <= horizontalSegments; h++)
                {
                    var ht = h / (float)horizontalSegments;
                    var phi = ht * 2 * Mathf.PI;

                    var x = radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                    var y = radius * Mathf.Cos(theta);
                    var z = radius * Mathf.Sin(theta) * Mathf.Sin(phi);

                    var vertex = new Vector3(x, y, z);
                    vertices.Add(vertex);
                    normals.Add(vertex.normalized);
                    uvs.Add(new Vector2(ht, vt));
                }
            }

            for (var v = 0; v < verticalSegments; v++)
            {
                for (var h = 0; h < horizontalSegments; h++)
                {
                    var current = v * (horizontalSegments + 1) + h;
                    var next = current + horizontalSegments + 1;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            return mesh;
        }

        private void GenerateLife()
        {
            if (lifePrefabs.Length == 0) return;
            if (!hasLife) return;

            if (!lifeManager)
            {
                lifeManager = gameObject.AddComponent<PlanetLifeManager>();
            }

            lifeManager.GenerateObjectPositions();
        }

        private void GenerateRocks()
        {
            if (rockPrefabs.Length == 0) return;
            if (numRocks == 0) return;

            if (!rockManager)
            {
                rockManager = gameObject.AddComponent<PlanetRockManager>();
            }

            rockManager.GenerateObjectPositions();
        }

        private void GenerateTrees()
        {
            if (treePrefabs.Length == 0) return;
            if (numTrees == 0) return;

            if (!treeManager)
            {
                treeManager = gameObject.AddComponent<PlanetTreeManager>();
            }

            treeManager.GenerateObjectPositions();
        }

        private void GenerateMesh()
        {
            for (var i = 0; i < 6; i++)
                if (_meshFilters[i].gameObject.activeSelf)
                    _terrainFaces[i].ConstructMesh();

            _colourGenerator.UpdateElevation(ShapeGenerator.ElevationMinMax);
        }

        private void GenerateColours()
        {
            _colourGenerator.UpdateColours();
            for (var i = 0; i < 6; i++)
                if (_meshFilters[i].gameObject.activeSelf)
                    _terrainFaces[i].UpdateUVs(_colourGenerator);
        }
    }
}