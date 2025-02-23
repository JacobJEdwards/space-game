using Managers;
using Unity.Serialization;
using Unity.VisualScripting;
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
        public Biome biome;

        [SerializeField]
        private float rockSpawnRadius = 800f;

        [SerializeField]
        private float rockCheckInterval = 1f;

        [DontSerialize] public ShapeSettings shapeSettings;
        [DontSerialize] public ColourSettings colourSettings;

        [SerializeField] public GameObject[] rockPrefabs;
        private GameObject[] _activeRocks;

        [HideInInspector] public bool shapeSettingsFoldout;
        [HideInInspector] public bool colourSettingsFoldout;

        [DontSerialize] private MeshFilter[] _meshFilters;

        [DontSerialize] private TerrainFace[] _terrainFaces;

        private GameObject _meshContainer;

        private GameObject _atmosphere;

        public readonly ColourGenerator ColourGenerator = new();
        public readonly ShapeGenerator ShapeGenerator = new();

        public GameObject[] lifePrefabs;

        [SerializeField] private PlanetRockManager rockManager;

        private void Initialize()
        {
            ShapeGenerator.UpdateSettings(shapeSettings);
            ColourGenerator.UpdateSettings(colourSettings);

            _meshContainer = new GameObject("PlanetMesh")
            {
                transform =
                {
                    parent = transform,
                    position = transform.position
                },
                layer = (int)Layers.PlanetSurface
            };

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

            foreach (var meshFilter in _meshFilters)
            {
                var col = meshFilter.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = meshFilter.sharedMesh;
            }
        }

        // TODO, make better
        public void GenerateLife()
        {
            if (lifePrefabs.Length == 0) return;

            for (var i = 0; i < 10; i++)
            {
                var prefab = lifePrefabs[Random.Range(0, lifePrefabs.Length)];
                var pos = Random.onUnitSphere;
                var heightAtPoint = ShapeGenerator.GetScaledElevation(ShapeGenerator.CalculateUnscaledElevation(pos));
                pos *= heightAtPoint * 1.1f;

                var life = Instantiate(prefab, pos, Quaternion.identity);
                life.transform.parent = transform;
                life.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
                life.transform.position += transform.position;

                var controller = life.GetComponent<NpcMovement>();
                controller.planet = transform;
                controller.target = playerTransform;
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

        private void GenerateRocks()
        {
            if (_activeRocks != null) {
                foreach (var rock in _activeRocks)
                    Destroy(rock);
                _activeRocks = null;
            }

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

            ColourGenerator.UpdateElevation(ShapeGenerator.ElevationMinMax);
        }

        private void GenerateColours()
        {
            ColourGenerator.UpdateColours();
            for (var i = 0; i < 6; i++)
                if (_meshFilters[i].gameObject.activeSelf)
                    _terrainFaces[i].UpdateUVs(ColourGenerator);
        }
    }
}