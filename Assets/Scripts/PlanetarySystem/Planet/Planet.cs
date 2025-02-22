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
        public bool autoUpdate = true;

        public FaceRenderMask faceRenderMask;
        public PlanetWater waterSystem;

        public Material waterMaterial;

        public ShapeSettings shapeSettings;
        public ColourSettings colourSettings;

        [HideInInspector] public bool shapeSettingsFoldout;
        [HideInInspector] public bool colourSettingsFoldout;

        [SerializeField] [HideInInspector] private MeshFilter[] meshFilters;

        [SerializeField] [HideInInspector] private TerrainFace[] terrainFaces;

        [SerializeField] [HideInInspector] private GameObject atmosphere;
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

            if (meshFilters == null || meshFilters.Length == 0) meshFilters = new MeshFilter[6];

            terrainFaces = new TerrainFace[6];

            Vector3[] directions =
                { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            for (var i = 0; i < 6; i++)
            {
                if (!meshFilters[i])
                {
                    var meshObj = new GameObject("mesh");

                    meshObj.transform.parent = transform;
                    meshObj.AddComponent<MeshRenderer>();

                    meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    meshFilters[i].sharedMesh = new Mesh();

                    var col = meshFilters[i].AddComponent<MeshCollider>();
                    col.sharedMesh = meshFilters[i].sharedMesh;
                    col.gameObject.layer = 9;
                }

                meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

                terrainFaces[i] =
                    new TerrainFace(_shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);

                var renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;

                meshFilters[i].gameObject.SetActive(renderFace);
            }

            if (!waterSystem)
            {
                var water = new GameObject("PlanetWater");
                water.transform.parent = transform;
                waterSystem = water.AddComponent<PlanetWater>();
            }

            waterSystem.GenerateWater(shapeSettings, waterMaterial);
        }

        public void GeneratePlanet()
        {
            Initialize();
            GenerateMesh();
            GenerateColours();
            gameObject.layer = 9;
            GenerateAtmosphere();
        }

        private void GenerateAtmosphere()
        {
            if (atmosphere) return;

            var atmos = new GameObject("Atmosphere");
            atmos.transform.parent = transform;

            var sphere = atmos.AddComponent<SphereCollider>();
            sphere.radius = shapeSettings.planetRadius * 1.3f;
            sphere.isTrigger = true;

            atmos.layer = 8;

            atmosphere = atmos;
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
                if (meshFilters[i].gameObject.activeSelf)
                    terrainFaces[i].ConstructMesh();

            _colourGenerator.UpdateElevation(_shapeGenerator.ElevationMinMax);
        }

        private void GenerateColours()
        {
            _colourGenerator.UpdateColours();
            for (var i = 0; i < 6; i++)
                if (meshFilters[i].gameObject.activeSelf)
                    terrainFaces[i].UpdateUVs(_colourGenerator);
        }
    }
}