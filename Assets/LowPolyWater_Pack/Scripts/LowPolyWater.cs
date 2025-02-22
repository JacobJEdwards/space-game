using UnityEngine;

namespace LowPolyWater
{
    public class LowPolyWater : MonoBehaviour
    {
        public float waveHeight = 0.5f;
        public float waveFrequency = 0.5f;
        public float waveLength = 0.75f;

        //Position where the waves originate from
        public Vector3 waveOriginPosition = new(0.0f, 0.0f, 0.0f);
        private Mesh mesh;

        private MeshFilter meshFilter;
        private Vector3[] vertices;

        private void Awake()
        {
            //Get the Mesh Filter of the gameobject
            meshFilter = GetComponent<MeshFilter>();
        }

        private void Start()
        {
            CreateMeshLowPoly(meshFilter);
        }

        private void Update()
        {
            GenerateWaves();
        }

        /// <summary>
        ///     Rearranges the mesh vertices to create a 'low poly' effect
        /// </summary>
        /// <param name="mf">Mesh filter of gamobject</param>
        /// <returns></returns>
        private MeshFilter CreateMeshLowPoly(MeshFilter mf)
        {
            mesh = mf.sharedMesh;

            //Get the original vertices of the gameobject's mesh
            var originalVertices = mesh.vertices;

            //Get the list of triangle indices of the gameobject's mesh
            var triangles = mesh.triangles;

            //Create a vector array for new vertices 
            var vertices = new Vector3[triangles.Length];

            //Assign vertices to create triangles out of the mesh
            for (var i = 0; i < triangles.Length; i++)
            {
                vertices[i] = originalVertices[triangles[i]];
                triangles[i] = i;
            }

            //Update the gameobject's mesh with new vertices
            mesh.vertices = vertices;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            this.vertices = mesh.vertices;

            return mf;
        }

        /// <summary>
        ///     Based on the specified wave height and frequency, generate
        ///     wave motion originating from waveOriginPosition
        /// </summary>
        private void GenerateWaves()
        {
            for (var i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];

                //Initially set the wave height to 0
                v.y = 0.0f;

                //Get the distance between wave origin position and the current vertex
                var distance = Vector3.Distance(v, waveOriginPosition);
                distance = distance % waveLength / waveLength;

                //Oscilate the wave height via sine to create a wave effect
                v.y = waveHeight * Mathf.Sin(Time.time * Mathf.PI * 2.0f * waveFrequency
                                             + Mathf.PI * 2.0f * distance);

                //Update the vertex
                vertices[i] = v;
            }

            //Update the mesh properties
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.MarkDynamic();
            meshFilter.mesh = mesh;
        }
    }
}