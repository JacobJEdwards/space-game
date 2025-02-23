using Unity.VisualScripting;
using UnityEngine;

namespace PlanetarySystem.Planet
{
    public class WaterFace
    {
        private readonly Vector3 _axisA;
        private readonly Vector3 _axisB;
        private readonly Vector3 _localUp;
        private readonly Mesh _mesh;
        private readonly float _radius;
        private readonly int _resolution;

        public WaterFace(Mesh mesh, int resolution, Vector3 localUp, float radius)
        {
            _mesh = mesh;
            _resolution = resolution;
            _localUp = localUp;
            _radius = radius;

            _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            _axisB = Vector3.Cross(localUp, _axisA);
        }

        public void ConstructMesh()
        {
            var vertices = new Vector3[_resolution * _resolution];
            var triangles = new int[(_resolution - 1) * (_resolution - 1) * 6];
            var triIndex = 0;

            for (var y = 0; y < _resolution; y++)
            for (var x = 0; x < _resolution; x++)
            {
                var i = x + y * _resolution;
                var percent = new Vector2(x, y) / (_resolution - 1);
                var pointOnUnitCube = _localUp + (percent.x - .5f) * 2 * _axisA + (percent.y - .5f) * 2 * _axisB;
                var pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = pointOnUnitSphere * _radius;

                if (x == _resolution - 1 || y == _resolution - 1) continue;

                triangles[triIndex] = i;
                triangles[triIndex + 1] = i + _resolution + 1;
                triangles[triIndex + 2] = i + _resolution;

                triangles[triIndex + 3] = i;
                triangles[triIndex + 4] = i + 1;
                triangles[triIndex + 5] = i + _resolution + 1;
                triIndex += 6;
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }
    }

    public class WaterPhysics : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var rb = other.GetComponent<Rigidbody>();

            if (!rb) return;

            rb.linearDamping = 3f;
            rb.angularDamping = 5f;
        }

        private void OnTriggerExit(Collider other)
        {
            var rb = other.GetComponent<Rigidbody>();

            if (!rb) return;

            rb.linearDamping = 0.5f;
            rb.angularDamping = 2f;
        }

        private void OnTriggerStay(Collider other)
        {
            var rb = other.GetComponent<Rigidbody>();

            if (!rb) return;

            rb.AddForce(Vector3.up * 9.81f, ForceMode.Acceleration);
        }
    }

    public class PlanetWater : MonoBehaviour
    {
        [Range(2, 256)] public int resolution = 30;
        public float waterLevel = 1.02f;

        private Transform _planetTransform;
        private WaterFace[] _waterFaces;
        private Material _waterMaterial;
        private MeshFilter[] _waterMeshFilters;

        private void Initialise(ShapeSettings planetShapeSettings)
        {
            _waterMeshFilters = new MeshFilter[6];
            _waterFaces = new WaterFace[6];

            Vector3[] directions =
                { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            for (var i = 0; i < 6; i++)
            {
                if (!_waterMeshFilters[i])
                {
                    var meshObj = new GameObject($"waterMesh_{i}")
                    {
                        transform =
                        {
                            parent = transform,
                            localPosition = Vector3.zero
                        }
                    };

                    var render = meshObj.AddComponent<MeshRenderer>();
                    _waterMeshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    _waterMeshFilters[i].sharedMesh = new Mesh();
                    // meshObj.AddComponent<LowPolyWater.LowPolyWater>(); - causes it to be circular, fix later

                    render.sharedMaterial = _waterMaterial;
                }

                _waterFaces[i] = new WaterFace(_waterMeshFilters[i].sharedMesh, resolution, directions[i],
                    planetShapeSettings.planetRadius * waterLevel);
            }
        }

        public void GenerateWater(ShapeSettings planetShapeSettings, Material waterMaterial)
        {
            var mainCollider = gameObject.AddComponent<SphereCollider>();
            mainCollider.radius = planetShapeSettings.planetRadius * waterLevel;
            mainCollider.isTrigger = true;
            mainCollider.gameObject.layer = (int)Layers.Water;
            mainCollider.AddComponent<WaterPhysics>();

            _waterMaterial = waterMaterial;
            Initialise(planetShapeSettings);
            GenerateWaterMesh();
        }

        private void GenerateWaterMesh()
        {
            foreach (var waterFace in _waterFaces)
            {
                waterFace.ConstructMesh();
            }
        }
    }
}