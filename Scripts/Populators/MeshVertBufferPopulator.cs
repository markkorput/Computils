using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
    class MeshVertBufferPopulator : MonoBehaviour
    {
        public enum MeshInterpretation { Vertices, Triangles };

        public ComputeBufferFacade Facade;
        public MeshFilter MeshFilter;
        public MeshInterpretation Interpretation = MeshInterpretation.Vertices;
        public float ScaleFactor = 1.0f;

        private void Start()
        {
            Vector3[] verts = null;
         
            if (this.Interpretation.Equals(MeshInterpretation.Vertices))
                verts = GetVerts(MeshFilter.mesh, this.ScaleFactor);

            if (this.Interpretation.Equals(MeshInterpretation.Triangles))
                verts = GetTriangleVerts(MeshFilter.mesh, this.ScaleFactor);

            if (verts != null)
            {
                ComputeBuffer buf = Utils.Create(verts);
                Facade.Set(buf);
            }
        }

        private static Vector3[] GetVerts(Mesh mesh, float ScaleFactor)
        {
            Vector3[] data = new Vector3[mesh.vertices.Length];

            for (int i = 0; i < mesh.vertices.Length; i++)
                data[i] = mesh.vertices[i] * ScaleFactor;

            return data;
        }

        private static Vector3[] GetTriangleVerts(Mesh mesh, float ScaleFactor)
        {
            int[] vertIndices = mesh.GetTriangles(0);
            Vector3[] data = new Vector3[vertIndices.Length];

            for (int i = 0; i < vertIndices.Length; i++)
                data[i] = mesh.vertices[vertIndices[i]] * ScaleFactor;

            return data;
        }
    }
}
