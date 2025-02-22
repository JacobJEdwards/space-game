using Unity.Serialization;
using Unity.VisualScripting;
using UnityEngine;

// TODO : SEED - RANDOM GEN
namespace PlanetarySystem.Planet
{
    public class Planet : MonoBehaviour
    {
        public enum FaceRenderMask
        {
            All,
            Top,
            Bottom,
            Left,
            Right,
            Front,
            Back
        }

        [Range(2, 256)] public int resolution = 10;
        [Range(0, 512)] public int numRocks = 40;
        public bool autoUpdate = true;

        public FaceRenderMask faceRenderMask = FaceRenderMask.All;
        public PlanetWater waterSystem;
        public Material waterMaterial;

        public bool hasWater;

        [DontSerialize]
        public ShapeSettings shapeSettings;
        [DontSerialize]
        public ColourSettings colourSettings;

        [SerializeField] public GameObject[] rockPrefabs;
        private GameObject[] _activeRocks;

        [HideInInspector] public bool shapeSettingsFoldout;
        [HideInInspector] public bool colourSettingsFoldout;

        [DontSerialize]
        private MeshFilter[] _meshFilters;

        [DontSerialize]
        private TerrainFace[] _terrainFaces;

        private GameObject _atmosphere;
        private readonly ColourGenerator _colourGenerator = new();

        private readonly ShapeGenerator _shapeGenerator = new();

        private void Start()
        {
            GeneratePlanet();
        }

        private void Initialize()
        {
            _shapeGenerator.UpdateSettings(shapeSettings);
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
                            parent = transform,
                            position = transform.position
                        }
                    };

                    meshObj.AddComponent<MeshRenderer>();
                    _meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    _meshFilters[i].sharedMesh = new Mesh();
                }

                _meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
                var meshCollider = _meshFilters[i].GetComponent<MeshCollider>();
                if (!meshCollider)
                {
                    meshCollider = _meshFilters[i].gameObject.AddComponent<MeshCollider>();
                }

                meshCollider.sharedMesh = _meshFilters[i].sharedMesh;


                _terrainFaces[i] = new TerrainFace(_shapeGenerator, _meshFilters[i].sharedMesh, resolution, directions[i]);

                var renderFace = faceRenderMask == FaceRenderMask.All || (int) faceRenderMask - 1 == i;
                _meshFilters[i].gameObject.SetActive(renderFace);
                _meshFilters[i].gameObject.layer = (int) Layers.PlanetSurface;

            }

            gameObject.layer = (int) Layers.PlanetSurface;
        }


        public void GeneratePlanet()
        {
            Initialize();
            GenerateMesh();
            GenerateColours();
            GenerateAtmosphere();
            if (hasWater) GenerateWater();
            GenerateRocks();
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
            sphere.radius = shapeSettings.planetRadius * 1.5f;
            sphere.isTrigger = true;

            _atmosphere.layer = (int)Layers.Atmosphere;
        }

        private void GenerateRocks()
        {
            if (_activeRocks != null)
                foreach (var rock in _activeRocks)
                    Destroy(rock);

            _activeRocks = new GameObject[numRocks];

            for (var i = 0; i < numRocks; i++)
            {
                // doesnt work ??
                var rock = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], transform);
                var pos = Random.onUnitSphere;

                // find position on surface based on mesh
                var heightAtPoint = _shapeGenerator.GetScaledElevation(_shapeGenerator.CalculateUnscaledElevation(pos));
                pos *= heightAtPoint;

                rock.transform.localPosition = pos;
                rock.transform.localScale = Vector3.one * Random.Range(2f, 10f);
                rock.transform.rotation = Random.rotation;
                _activeRocks[i] = rock;
            }
        }

        public void OnShapeSettingsUpdated()
        {
            if (!autoUpdate) return;

            Initialize();
            GenerateMesh();
        }

        public void OnColourSettingsUpdated()
        {
            if (!autoUpdate) return;

            Initialize();
            GenerateColours();
        }

        private void GenerateMesh()
        {
            for (var i = 0; i < 6; i++)
                if (_meshFilters[i].gameObject.activeSelf)
                    _terrainFaces[i].ConstructMesh();

            _colourGenerator.UpdateElevation(_shapeGenerator.ElevationMinMax);
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