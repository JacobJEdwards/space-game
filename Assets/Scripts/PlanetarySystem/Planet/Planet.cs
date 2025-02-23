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

        public PlanetWater waterSystem;
        public Material waterMaterial;

        public Transform playerTransform;

        public bool hasWater;
        public bool hasLife;

        public Biome biome;

        [DontSerialize] public ShapeSettings shapeSettings;
        [DontSerialize] public ColourSettings colourSettings;

        [SerializeField] public Rock[] rockPrefabs;
        [SerializeField] public Life[] lifePrefabs;

        [HideInInspector] public bool shapeSettingsFoldout;
        [HideInInspector] public bool colourSettingsFoldout;

        [DontSerialize] private MeshFilter[] _meshFilters;

        [DontSerialize] private TerrainFace[] _terrainFaces;

        private GameObject _atmosphere;

        private readonly ColourGenerator _colourGenerator = new();
        public readonly ShapeGenerator ShapeGenerator = new();


        [SerializeField] private PlanetRockManager rockManager;
        [SerializeField] private PlanetLifeManager lifeManager;

        private void Initialize()
        {
            ShapeGenerator.UpdateSettings(shapeSettings);
            _colourGenerator.UpdateSettings(colourSettings);

            if (_meshFilters == null || _meshFilters.Length == 0) _meshFilters = new MeshFilter[6];

            _terrainFaces = new TerrainFace[6];

            Vector3[] directions =
                { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            for (var i = 0; i < 6; i++)
            {
                if (!_meshFilters[i])
                {
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
                }

                _meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

                _terrainFaces[i] =
                    new TerrainFace(ShapeGenerator, _meshFilters[i].sharedMesh, resolution, directions[i]);
                _meshFilters[i].gameObject.layer = (int)Layers.PlanetSurface;

            }

            gameObject.layer = (int)Layers.PlanetSurface;
        }

        public void GeneratePlanet()
        {
            Initialize();
            GenerateMesh();
            GenerateColours();
            GenerateAtmosphere();
            if (hasWater) GenerateWater();
            GenerateRocks();
            GenerateLife();

            foreach (var meshFilter in _meshFilters)
            {
                var col = meshFilter.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = meshFilter.sharedMesh;
            }
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
                    position = transform.position
                }
            };


            var sphere = _atmosphere.AddComponent<SphereCollider>();
            sphere.radius = shapeSettings.planetRadius * 1.8f;
            sphere.isTrigger = true;

            _atmosphere.layer = (int)Layers.Atmosphere;
        }

        private void GenerateLife()
        {
            if (lifePrefabs.Length == 0)
            {
                return;
            }
            if (!hasLife) return;

            if (!lifeManager)
            {
                lifeManager = gameObject.AddComponent<PlanetLifeManager>();
            }

            lifeManager.GenerateLifeSpawns();
        }

        private void GenerateRocks()
        {
            if (rockPrefabs.Length == 0) return;
            if (numRocks == 0) return;

            if (!rockManager)
            {
                rockManager = gameObject.AddComponent<PlanetRockManager>();
            }

            rockManager.GenerateRockPositions();
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